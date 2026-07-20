#import <UIKit/UIKit.h>

static UIImpactFeedbackGenerator *cbcLightGenerator = nil;
static UIImpactFeedbackGenerator *cbcHeavyGenerator = nil;
static UIImpactFeedbackGenerator *cbcSoftGenerator = nil;

extern "C" {

void _CBCHapticsPrepare(void)
{
    if (@available(iOS 13.0, *)) {
        if (cbcLightGenerator == nil) {
            cbcLightGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
        }
        if (cbcHeavyGenerator == nil) {
            cbcHeavyGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
        }
        if (cbcSoftGenerator == nil) {
            cbcSoftGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleSoft];
        }

        [cbcLightGenerator prepare];
        [cbcHeavyGenerator prepare];
        [cbcSoftGenerator prepare];
    }
}

void _CBCHapticsPlay(int type)
{
    if (@available(iOS 13.0, *)) {
        UIImpactFeedbackGenerator *generator = nil;

        switch (type) {
            case 1:  generator = cbcHeavyGenerator; break;
            case 2:  generator = cbcSoftGenerator;  break;
            default: generator = cbcLightGenerator; break;
        }

        if (generator != nil) {
            [generator impactOccurred];
            [generator prepare];
        }
    }
}

}
