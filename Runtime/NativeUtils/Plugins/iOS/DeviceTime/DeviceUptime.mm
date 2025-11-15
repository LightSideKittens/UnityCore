#import <mach/mach_time.h>
#import <Foundation/Foundation.h>
#import <sys/types.h>
#import <sys/sysctl.h>
#import <sys/time.h>

extern "C" uint64_t DeviceGetUptimeTicks()
{
    return mach_absolute_time();
}

extern "C" long long DeviceGetBootSessionId()
{
    struct timeval boottime;
    size_t size = sizeof(boottime);

    int mib[2];
    mib[0] = CTL_KERN;
    mib[1] = KERN_BOOTTIME;

    if (sysctl(mib, 2, &boottime, &size, NULL, 0) != 0 || boottime.tv_sec == 0)
        return 0;

    long long ms = (long long)boottime.tv_sec * 1000LL + boottime.tv_usec / 1000LL;
    return ms;
}