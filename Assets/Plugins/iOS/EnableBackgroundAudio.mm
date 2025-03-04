#import <AVFoundation/AVFoundation.h>

@interface EnableBackgroundAudioWrapper : NSObject
+ (void)enableBackgroundAudio;
@end

@implementation EnableBackgroundAudioWrapper

+ (void)enableBackgroundAudio {
    AVAudioSession* audioSession = [AVAudioSession sharedInstance];
    [audioSession setCategory:AVAudioSessionCategoryPlayback error:nil];
    // [audioSession setActive:YES error:nil];  // 追加したが、それでもバックグラウンド再生中は音が出ない
}

@end


#ifdef __cplusplus
extern "C" {
#endif

void __enableBackgroundAudio() {
    return [EnableBackgroundAudioWrapper enableBackgroundAudio];
}

#ifdef __cplusplus
}
#endif
