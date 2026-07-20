using System.Collections.Generic;
using System.Threading;
using App.Scripts.Core.Common;
using App.Scripts.Domain.Match;
using App.Scripts.Engine.Audio;
using App.Scripts.Engine.Haptics;
using App.Scripts.Gameplay.Levels;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace App.Scripts.Gameplay.View
{
    public sealed class GameplayPresenter : MonoBehaviour
    {
        private const int BLOCK_PREWARM = 128;
        private const int UNIT_PREWARM = 12;
        private const int PROJECTILE_PREWARM = 16;
        private const float UNIT_EXIT_DISTANCE = 12f;
        private const float UNIT_EXIT_DURATION = 0.45f;
        private const float DESTROY_SOUND_INTERVAL = 0.07f;
        private const float SHOT_SOUND_INTERVAL = 0.06f;
        private const string SOUND_DEPLOY = "ButtonClick";
        private const string SOUND_REJECTED = "PanelClose";
        private const string SOUND_DESTROY = "UpgradeClick";
        private const string SOUND_SHOT = "Laser";
        private const string SOUND_WON = "PanelOpen";
        private const string SOUND_LOST = "PanelClose";

        [SerializeField] private GameplayLayout _layout;
        [SerializeField] private GameplayPalette _palette;
        [SerializeField] private GameplayInput _input;
        [SerializeField] private BlockView _blockPrefab;
        [SerializeField] private UnitView _unitPrefab;
        [SerializeField] private ProjectileView _projectilePrefab;
        [SerializeField] private SlotMarkerView _slotMarkerPrefab;
        [SerializeField] private ParticleSystem _destroyEffect;
        [SerializeField] private Transform _boardRoot;
        [SerializeField] private Transform _queueRoot;
        [SerializeField] private Transform _projectileRoot;
        [SerializeField] private float _tapRadius = 0.8f;

        private readonly Dictionary<int, BlockView> _blocks = new Dictionary<int, BlockView>(256);
        private readonly Dictionary<int, UnitView> _units = new Dictionary<int, UnitView>(32);
        private readonly List<MatchEvent> _eventBuffer = new List<MatchEvent>(64);
        private readonly List<ProjectileView> _projectiles = new List<ProjectileView>(16);
        private readonly List<UnitView> _exiting = new List<UnitView>(8);
        private readonly SlotMarkerView[] _slotMarkers = new SlotMarkerView[SlotBoard.SLOT_COUNT];
        private readonly List<float> _exitTimers = new List<float>(8);

        private readonly Subject<Unit> _won = new Subject<Unit>();
        private readonly Subject<Unit> _lost = new Subject<Unit>();
        private readonly ReactiveProperty<float> _progress = new ReactiveProperty<float>(0f);

        private ILevelService _levelService;
        private GameSessionService _session;
        private IHapticsService _haptics;
        private IAudioService _audio;
        private float _lastDestroySoundTime = float.NegativeInfinity;
        private float _lastShotSoundTime = float.NegativeInfinity;
        private MatchSimulation _simulation;
        private ObjectPool<BlockView> _blockPool;
        private ObjectPool<UnitView> _unitPool;
        private ObjectPool<ProjectileView> _projectilePool;
        private int _lastDestroyedCellId = -1;
        private bool _running;

        public Observable<Unit> OnWon => _won;
        public Observable<Unit> OnLost => _lost;
        public ReadOnlyReactiveProperty<float> Progress => _progress;
        public int CurrentLevelId => _levelService.CurrentLevelId;

        [Inject]
        public void Construct(
            ILevelService levelService,
            GameSessionService session,
            IHapticsService haptics,
            IAudioService audioService)
        {
            _levelService = levelService;
            _session = session;
            _haptics = haptics;
            _audio = audioService;
        }

        private void Awake()
        {
            Anim.Init();
            CreatePools();
            CreateSlotMarkers();
        }

        private void Start()
        {
            StartSessionAsync().Forget(Log.Exception);
        }

        private void PlaySound(string key)
        {
            _audio.PlaySfx(key);
        }

        private void PlayShotSound()
        {
            if (Time.unscaledTime - _lastShotSoundTime < SHOT_SOUND_INTERVAL)
                return;

            _lastShotSoundTime = Time.unscaledTime;
            _audio.PlaySfx(SOUND_SHOT);
        }

        private void PlayDestroySound()
        {
            if (Time.unscaledTime - _lastDestroySoundTime < DESTROY_SOUND_INTERVAL)
                return;

            _lastDestroySoundTime = Time.unscaledTime;
            _audio.PlaySfx(SOUND_DESTROY);
        }

        private async UniTask StartSessionAsync()
        {
            await _levelService.InitializeAsync(destroyCancellationToken);

            _session.Attach(this);

            await StartLevelAsync(destroyCancellationToken);
            _session.NotifyLevelStarted();
        }

        private void OnDestroy()
        {
            _session.Detach();
            _won.Dispose();
            _lost.Dispose();
            _progress.Dispose();
        }

        public async UniTask StartLevelAsync(CancellationToken ct = default)
        {
            Teardown();

            LevelDefinition level = await _levelService.LoadCurrentAsync(ct);
            _simulation = MatchBuilder.Build(level, MatchConfig.CreateDefault(), Random.Range(int.MinValue, int.MaxValue));

            _layout.Configure(level.Columns, level.Cells.Count, level.Lanes.Count);

            SpawnBoard();
            SpawnQueue();
            LayoutSlotMarkers();

            _progress.Value = 0f;
            _running = true;
        }

        private void Update()
        {
            if (!_running)
                return;

            float deltaTime = Time.deltaTime;

            HandleInput();

            _simulation.Tick(deltaTime);
            _simulation.ConsumeEvents(_eventBuffer);
            ProcessEvents();

            AdvanceProjectiles(deltaTime);
            AdvanceExits(deltaTime);

            _progress.Value = _simulation.Progress;
        }

        private void HandleInput()
        {
            if (!_input.TryGetTapWorldPosition(out Vector3 tapPosition))
                return;

            for (int laneIndex = 0; laneIndex < _simulation.LaneCount; laneIndex++)
            {
                QueueLane lane = _simulation.GetLane(laneIndex);

                if (lane.IsEmpty)
                    continue;

                Vector3 headPosition = _layout.GetQueuePosition(laneIndex, 0);

                if (Vector3.Distance(headPosition, tapPosition) > _tapRadius)
                    continue;

                _simulation.TryDeploy(laneIndex);
                return;
            }
        }

        private void ProcessEvents()
        {
            for (int i = 0; i < _eventBuffer.Count; i++)
            {
                MatchEvent matchEvent = _eventBuffer[i];

                switch (matchEvent.Type)
                {
                    case MatchEventType.UnitDeployed:
                        HandleUnitDeployed(matchEvent);
                        _haptics.Play(HapticType.Light);
                        PlaySound(SOUND_DEPLOY);
                        break;
                    case MatchEventType.DeployRejected:
                        _haptics.Play(HapticType.Soft);
                        PlaySound(SOUND_REJECTED);
                        break;
                    case MatchEventType.ShotFired:
                        HandleShotFired(matchEvent);
                        break;
                    case MatchEventType.PieceDestroyed:
                        HandlePieceDestroyed(matchEvent);
                        _haptics.Play(HapticType.Soft);
                        PlayDestroySound();
                        break;
                    case MatchEventType.LayerRevealed:
                        RefreshBlock(matchEvent.CellId);
                        break;
                    case MatchEventType.ColumnCollapsed:
                        HandleColumnCollapsed(matchEvent);
                        break;
                    case MatchEventType.UnitFinished:
                        HandleUnitFinished(matchEvent);
                        break;
                    case MatchEventType.Won:
                        _running = false;
                        _haptics.Play(HapticType.Heavy);
                        PlaySound(SOUND_WON);
                        _won.OnNext(Unit.Default);
                        break;
                    case MatchEventType.Lost:
                        _running = false;
                        _haptics.Play(HapticType.Heavy);
                        PlaySound(SOUND_LOST);
                        _lost.OnNext(Unit.Default);
                        break;
                }
            }
        }

        private void HandleUnitDeployed(MatchEvent matchEvent)
        {
            if (!_units.TryGetValue(matchEvent.UnitId, out UnitView view))
                return;

            Anim.JumpTo(view.transform, _layout.GetSlotPosition(matchEvent.SlotIndex), matchEvent.Duration);
            view.SetAvailable(true);
            _slotMarkers[matchEvent.SlotIndex].SetOccupied(true);

            LayoutLane(matchEvent.LaneIndex);
        }

        private void HandleShotFired(MatchEvent matchEvent)
        {
            if (_units.TryGetValue(matchEvent.UnitId, out UnitView unitView))
            {
                unitView.SetCharges(matchEvent.Charges);
                unitView.PlayFire();
            }

            PlayShotSound();

            if (!_blocks.TryGetValue(matchEvent.CellId, out BlockView blockView))
                return;

            ProjectileView projectile = _projectilePool.Get();
            projectile.Launch(_layout.GetSlotPosition(matchEvent.SlotIndex), blockView.transform.position, matchEvent.Duration);
            _projectiles.Add(projectile);
        }

        private void HandlePieceDestroyed(MatchEvent matchEvent)
        {
            _lastDestroyedCellId = matchEvent.CellId;

            if (!_blocks.TryGetValue(matchEvent.CellId, out BlockView view))
                return;

            view.PlayHit();
            PlayDestroyEffect(view.transform.position, _palette.GetColor(matchEvent.Color));
        }

        private void HandleColumnCollapsed(MatchEvent matchEvent)
        {
            if (_lastDestroyedCellId >= 0 && _blocks.TryGetValue(_lastDestroyedCellId, out BlockView removed))
            {
                _blocks.Remove(_lastDestroyedCellId);
                _blockPool.Release(removed);
            }

            _lastDestroyedCellId = -1;

            BoardColumn column = _simulation.Board.GetColumn(matchEvent.Column);

            for (int row = 0; row < column.Count; row++)
            {
                if (!_blocks.TryGetValue(column[row].Id, out BlockView view))
                    continue;

                Anim.FallTo(view.transform, _layout.GetCellPosition(matchEvent.Column, row), matchEvent.Duration);
            }
        }

        private void HandleUnitFinished(MatchEvent matchEvent)
        {
            if (!_units.TryGetValue(matchEvent.UnitId, out UnitView view))
                return;

            _units.Remove(matchEvent.UnitId);
            _slotMarkers[matchEvent.SlotIndex].SetOccupied(false);
            Anim.SlideOut(view.transform, UNIT_EXIT_DISTANCE, UNIT_EXIT_DURATION);

            _exiting.Add(view);
            _exitTimers.Add(UNIT_EXIT_DURATION);
        }

        private void RefreshBlock(int cellId)
        {
            if (!_blocks.TryGetValue(cellId, out BlockView view))
                return;

            BoardCell cell = FindCell(cellId);

            if (cell == null)
                return;

            ApplyCellColors(view, cell);
        }

        private BoardCell FindCell(int cellId)
        {
            for (int columnIndex = 0; columnIndex < _simulation.Board.ColumnCount; columnIndex++)
            {
                BoardColumn column = _simulation.Board.GetColumn(columnIndex);

                for (int row = 0; row < column.Count; row++)
                {
                    if (column[row].Id == cellId)
                        return column[row];
                }
            }

            return null;
        }

        private void ApplyCellColors(BlockView view, BoardCell cell)
        {
            view.SetTopColor(_palette.GetColor(cell.TopColor));

            bool covered = cell.Depth > 1;
            Color coveredColor = covered
                ? _palette.GetCoveredColor(cell.ColorAt(cell.Depth - 2))
                : Color.black;

            view.SetCovered(covered, coveredColor);
        }

        private void CreateSlotMarkers()
        {
            for (int i = 0; i < SlotBoard.SLOT_COUNT; i++)
                _slotMarkers[i] = Instantiate(_slotMarkerPrefab, _queueRoot);
        }

        private void LayoutSlotMarkers()
        {
            for (int i = 0; i < SlotBoard.SLOT_COUNT; i++)
            {
                _slotMarkers[i].transform.position = _layout.GetSlotPosition(i);
                _slotMarkers[i].SetOccupied(false);
            }
        }

        private void SpawnBoard()
        {
            for (int columnIndex = 0; columnIndex < _simulation.Board.ColumnCount; columnIndex++)
            {
                BoardColumn column = _simulation.Board.GetColumn(columnIndex);

                for (int row = 0; row < column.Count; row++)
                {
                    BoardCell cell = column[row];
                    BlockView view = _blockPool.Get();

                    view.Bind(cell.Id);
                    view.transform.position = _layout.GetCellPosition(columnIndex, row);
                    ApplyCellColors(view, cell);

                    _blocks.Add(cell.Id, view);
                }
            }
        }

        private void SpawnQueue()
        {
            for (int laneIndex = 0; laneIndex < _simulation.LaneCount; laneIndex++)
            {
                QueueLane lane = _simulation.GetLane(laneIndex);

                for (int position = 0; position < lane.Count; position++)
                {
                    UnitModel unit = lane[position];
                    UnitView view = _unitPool.Get();

                    view.Bind(unit.Id, laneIndex, _palette.GetColor(unit.Color), unit.RemainingCharges);
                    view.transform.position = _layout.GetQueuePosition(laneIndex, position);
                    view.SetAvailable(position == 0);

                    _units.Add(unit.Id, view);
                }
            }
        }

        private void LayoutLane(int laneIndex)
        {
            QueueLane lane = _simulation.GetLane(laneIndex);

            for (int position = 0; position < lane.Count; position++)
            {
                if (!_units.TryGetValue(lane[position].Id, out UnitView view))
                    continue;

                Anim.MoveTo(view.transform, _layout.GetQueuePosition(laneIndex, position), 0.18f);
                view.SetAvailable(position == 0);
            }
        }

        private void AdvanceProjectiles(float deltaTime)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                ProjectileView projectile = _projectiles[i];
                projectile.Advance(deltaTime);

                if (!projectile.IsFinished)
                    continue;

                _projectiles.RemoveAt(i);
                _projectilePool.Release(projectile);
            }
        }

        private void AdvanceExits(float deltaTime)
        {
            for (int i = _exitTimers.Count - 1; i >= 0; i--)
            {
                _exitTimers[i] -= deltaTime;

                if (_exitTimers[i] > 0f)
                    continue;

                _unitPool.Release(_exiting[i]);
                _exiting.RemoveAt(i);
                _exitTimers.RemoveAt(i);
            }
        }

        private void PlayDestroyEffect(Vector3 position, Color color)
        {
            if (_destroyEffect == null)
                return;

            ParticleSystem.MainModule main = _destroyEffect.main;
            main.startColor = color;
            _destroyEffect.transform.position = position;
            _destroyEffect.Emit(12);
        }

        private void Teardown()
        {
            _running = false;

            foreach (KeyValuePair<int, BlockView> pair in _blocks)
                _blockPool.Release(pair.Value);

            foreach (KeyValuePair<int, UnitView> pair in _units)
                _unitPool.Release(pair.Value);

            _blocks.Clear();
            _units.Clear();

            for (int i = 0; i < _projectiles.Count; i++)
                _projectilePool.Release(_projectiles[i]);

            _projectiles.Clear();

            for (int i = 0; i < _exiting.Count; i++)
                _unitPool.Release(_exiting[i]);

            _exiting.Clear();
            _exitTimers.Clear();
            _lastDestroyedCellId = -1;
        }

        private void CreatePools()
        {
            _blockPool = new ObjectPool<BlockView>(
                () => Instantiate(_blockPrefab, _boardRoot),
                view => view.gameObject.SetActive(true),
                view =>
                {
                    view.ResetView();
                    view.gameObject.SetActive(false);
                },
                view => Destroy(view.gameObject),
                true,
                BLOCK_PREWARM,
                512);

            _unitPool = new ObjectPool<UnitView>(
                () => Instantiate(_unitPrefab, _queueRoot),
                view => view.gameObject.SetActive(true),
                view =>
                {
                    view.ResetView();
                    view.gameObject.SetActive(false);
                },
                view => Destroy(view.gameObject),
                true,
                UNIT_PREWARM,
                64);

            _projectilePool = new ObjectPool<ProjectileView>(
                () => Instantiate(_projectilePrefab, _projectileRoot),
                view => view.gameObject.SetActive(true),
                view => view.gameObject.SetActive(false),
                view => Destroy(view.gameObject),
                true,
                PROJECTILE_PREWARM,
                64);

            Prewarm();
        }

        private void Prewarm()
        {
            List<BlockView> blocks = new List<BlockView>(BLOCK_PREWARM);

            for (int i = 0; i < BLOCK_PREWARM; i++)
                blocks.Add(_blockPool.Get());

            for (int i = 0; i < blocks.Count; i++)
                _blockPool.Release(blocks[i]);

            List<UnitView> units = new List<UnitView>(UNIT_PREWARM);

            for (int i = 0; i < UNIT_PREWARM; i++)
                units.Add(_unitPool.Get());

            for (int i = 0; i < units.Count; i++)
                _unitPool.Release(units[i]);

            List<ProjectileView> projectiles = new List<ProjectileView>(PROJECTILE_PREWARM);

            for (int i = 0; i < PROJECTILE_PREWARM; i++)
                projectiles.Add(_projectilePool.Get());

            for (int i = 0; i < projectiles.Count; i++)
                _projectilePool.Release(projectiles[i]);
        }
    }
}
