set currentDir=%cd%
set loop=0
set page=1

cd C:/Users/Onsla/AppData/Local/Android/Sdk/platform-tools

rem EV_ABS 3
rem EV_SYN 0
rem ABS_MT_TRACKING_ID 57 - ID of the touch. important for multi-touch reports
rem ABS_MT_POSITION_X 53 - x coordinate of the touch
rem ABS_MT_POSITION_Y 54 - y coordinate of the touch
rem ABS_MT_TOUCH_MAJOR 48 - basically width of your finger tip in pixels
rem ABS_MT_PRESSURE 58 - pressure of the touch
rem SYN_MT_REPORT 2 - end of separate touch data
rem SYN_REPORT 0 - end of report

rem adb.exe shell sendevent /dev/input/event1 3 57 0
rem rem adb.exe shell sendevent /dev/input/event1 3 53 2500
rem rem adb.exe shell sendevent /dev/input/event1 3 54 500
rem rem adb.exe shell sendevent /dev/input/event1 3 48 5
rem rem adb.exe shell sendevent /dev/input/event1 3 58 50
rem adb.exe shell sendevent /dev/input/event1 0 2 0
rem adb.exe shell sendevent /dev/input/event1 0 0 0
rem rem adb.exe shell sleep 
rem adb.exe shell sendevent /dev/input/event0 3 57 -1
rem adb.exe shell sendevent /dev/input/event1 0 2 0
rem adb.exe shell sendevent /dev/input/event1 0 0 0

rem adb.exe shell sendevent /dev/input/event1 3 48 5
rem adb.exe shell sendevent /dev/input/event1 3 58 50
rem adb.exe shell sendevent /dev/input/event1 0 2 0
rem adb.exe shell sendevent /dev/input/event1 0 0 0
rem adb.exe shell sleep 3
rem adb.exe shell sendevent /dev/input/event1 3 53 500
rem adb.exe shell sendevent /dev/input/event1 3 54 421
rem adb.exe shell sendevent /dev/input/event1 3 48 5
rem adb.exe shell sendevent /dev/input/event1 0 0 0
rem adb.exe shell sleep 3
rem adb.exe shell sendevent /dev/input/event1 0 2 0
rem adb.exe shell sendevent /dev/input/event1 3 57 -1
rem adb.exe shell input tap 500 500

adb.exe shell sendevent /dev/input/event1 3 57 4830
adb.exe shell sendevent /dev/input/event1 1 330 1
adb.exe shell sendevent /dev/input/event1 1 325 1
adb.exe shell sendevent /dev/input/event1 3 53 3709
adb.exe shell sendevent /dev/input/event1 3 54 3302
adb.exe shell sendevent /dev/input/event1 3 48 5
adb.exe shell sendevent /dev/input/event1 3 49 5
adb.exe shell sendevent /dev/input/event1 0 0 0
adb.exe shell sendevent /dev/input/event1 3 54 3301
adb.exe shell sendevent /dev/input/event1 3 48 7
adb.exe shell sendevent /dev/input/event1 3 49 7
adb.exe shell sendevent /dev/input/event1 0 0 0
adb.exe shell sendevent /dev/input/event1 3 53 3656
adb.exe shell sendevent /dev/input/event1 3 54 3263
adb.exe shell sendevent /dev/input/event1 0 0 0
adb.exe shell sendevent /dev/input/event1 3 54 859
adb.exe shell sendevent /dev/input/event1 3 48 14
adb.exe shell sendevent /dev/input/event1 3 49 14
adb.exe shell sendevent /dev/input/event1 0 0 0
adb.exe shell sendevent /dev/input/event1 3 54 857
adb.exe shell sendevent /dev/input/event1 3 48 13
adb.exe shell sendevent /dev/input/event1 3 49 13
adb.exe shell sendevent /dev/input/event1 0 0 0
adb.exe shell sendevent /dev/input/event1 3 57 -1

adb.exe shell input tap 500 500

cd %currentDir%

