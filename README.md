# Mgr-21 Process Killer

![image1](https://raw.githubusercontent.com/mirzaevolution/Mgr21-Process-Killer/master/ScreenShots/images/0.PNG)

**Mgr-21 Process Killer** is an advanced process killer software that works for MS Windows Operating System. This process killer is totally different than others in the way it finds processes. It doesn't find processes by their names only, but with additional algorithm called "2 KB Initial Bytes" that can seek out all processes that are same but with different names. 

Why is it important? when you run 2 same programs but with different names(MyCalculator.exe, MyCalculator1.exe), they will have different process names too and suppose you would like to kill MyCalculator.exe, the counterpart won't be killed because of different process name. So, Find process by name won't be so effective in this matter. Therefore, **Mgr-21 Process Killer** uses different technique to find same processes with different names by scanning their 2 KB initial bytes and process names using parallel processing (2 cores+) or synchronous processing. 

**Mgr-21 Process Killer** comes with 2 programs to handle input data, remove data, behavior settings and security. They are GUI version and Console version. For GUI version it uses asynchronous programming to handle heavy loading so it won't block main UI and it also supports 2 languages, en-US (english us) and id-ID (indonesian) you can switch between the two easily without closing the program. For console version, it doesn't use arguments based invocation or command line arguments. But, it uses input-choice based commands (with proper validations) that can avoid user typing many commands.

![image2](https://raw.githubusercontent.com/mirzaevolution/Mgr21-Process-Killer/master/ScreenShots/images/1.PNG)


![image3](https://raw.githubusercontent.com/mirzaevolution/Mgr21-Process-Killer/master/ScreenShots/images/2.PNG)


To add files (.exe files), you can browse multiple files directly (either using GUI or Console) but with one exception, you cannot add files that are already on the list.

![image4](https://raw.githubusercontent.com/mirzaevolution/Mgr21-Process-Killer/master/ScreenShots/images/3.PNG)

You can also change timer interval to check the blocked processes. By default, it uses 1000 ms. You can only set to 500 ms for minimum. Why? the smaller value you set, the higher **process killer** will consume your cpu usage (more than necessary) and that's not a good idea.

for the security part, you must set master password to secure your data. Everytime you run either GUI or Console version, you'll be prompted a login field. It uses Windows Data Proctection (DP API) + Advanced Encryption Standard (AES) to secure all the required files.

![image5](https://raw.githubusercontent.com/mirzaevolution/Mgr21-Process-Killer/master/ScreenShots/images/4.PNG)

![image6](https://raw.githubusercontent.com/mirzaevolution/Mgr21-Process-Killer/master/ScreenShots/images/5.PNG)

![image7](https://raw.githubusercontent.com/mirzaevolution/Mgr21-Process-Killer/master/ScreenShots/images/6.PNG)

![image8](https://raw.githubusercontent.com/mirzaevolution/Mgr21-Process-Killer/master/ScreenShots/images/7.PNG)

![image9](https://raw.githubusercontent.com/mirzaevolution/Mgr21-Process-Killer/master/ScreenShots/images/8.PNG)

![image10](https://raw.githubusercontent.com/mirzaevolution/Mgr21-Process-Killer/master/ScreenShots/images/9.PNG)



### ![LIVE DEMO](https://youtu.be/GB5-JKdtR5s)



**NOTE:** You can use this Process Killer without opening the programs (suppose black list already defined). It'll automatically run when windows starts.



**You can contribute to this project to improve features and fix bugs.**

Are you ready to taste **Mgr-21 Process Killer** ??? Hit download link below.

## [DOWNLOAD](https://github.com/mirzaevolution/Mgr21-Process-Killer/releases/download/1.0.0/setup.exe)


Best Regards,


**Mirza Ghulam Rasyid**
