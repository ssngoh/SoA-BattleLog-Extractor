set currentDir=%cd%
set loop=0
set page=1
set totalPages=%1
set /a totalPages=%totalPages%+1

rem Set this to your own platform-tools directory
cd C:/Users/Onsla/AppData/Local/Android/Sdk/platform-tools 


mkdir "%currentDir%/Images/%page%"

:ScrollDownLoop
adb.exe shell sleep 2
adb.exe exec-out screencap -p > test.png
move test.png "%currentDir%/Images/%page%/%loop%.png"
rem if %loop% GTR 14 goto SelectNextPage. Used in pre v8.0.0

rem 125 is just a hardcoded number which I think will capture everything in a page
if %loop% GTR 125 goto SelectNextPage
set /a loop=%loop%+1

adb.exe shell sendevent /dev/input/event1 3 57 4830
adb.exe shell sendevent /dev/input/event1 1 330 1
adb.exe shell sendevent /dev/input/event1 1 325 1
adb.exe shell sendevent /dev/input/event1 3 53 3709
adb.exe shell sendevent /dev/input/event1 3 54 3302
adb.exe shell sendevent /dev/input/event1 3 48 25
adb.exe shell sendevent /dev/input/event1 3 49 25
adb.exe shell sendevent /dev/input/event1 0 0 0
adb.exe shell sendevent /dev/input/event1 3 53 3709
adb.exe shell sendevent /dev/input/event1 3 54 3302
adb.exe shell sendevent /dev/input/event1 3 48 25
adb.exe shell sendevent /dev/input/event1 3 49 25
adb.exe shell sendevent /dev/input/event1 0 0 0
adb.exe shell sendevent /dev/input/event1 3 53 3656
adb.exe shell sendevent /dev/input/event1 3 54 3263
adb.exe shell sendevent /dev/input/event1 0 0 0
adb.exe shell sendevent /dev/input/event1 3 53 3656
adb.exe shell sendevent /dev/input/event1 3 54 3263
adb.exe shell sendevent /dev/input/event1 0 0 0
adb.exe shell sendevent /dev/input/event1 3 54 857
adb.exe shell sendevent /dev/input/event1 3 48 25
adb.exe shell sendevent /dev/input/event1 3 49 25
adb.exe shell sendevent /dev/input/event1 0 0 0
adb.exe shell sendevent /dev/input/event1 3 54 857
adb.exe shell sendevent /dev/input/event1 3 48 25
adb.exe shell sendevent /dev/input/event1 3 49 25
adb.exe shell sendevent /dev/input/event1 0 0 0
adb.exe shell sendevent /dev/input/event1 3 57 -1
adb.exe shell sendevent /dev/input/event1 0 0 0
goto ScrollDownLoop


:SelectNextPage
set /a loop=0
set /a page=%page%+1
if %page% LSS %totalPages% mkdir "%currentDir%/Images/%page%"
adb.exe shell input tap 687 1648
rem if 8 seconds is not enough to go to the next page, just increase the value
adb.exe shell sleep 8
if %page% LSS %totalPages% goto ScrollDownLoop

rem comment this away if you don't want to reboot your phone. Since you'll most likely be running this thoughout the night
adb.exe shell reboot 

cd %currentDir%
ImageTrim.bat %1

