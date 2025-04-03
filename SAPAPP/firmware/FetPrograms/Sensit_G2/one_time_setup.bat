@echo off
SETLOCAL
SETLOCAL ENABLEDELAYEDEXPANSION


  
    CMD /C dpinst_64_eng.exe /SE /SW /SA /PATH %~dp0\ccs_base/emulation/drivers/msp430/USB_CDC
  

  
    CMD /C dpinst_64_eng.exe /SE /SW /SA /PATH %~dp0\ccs_base/emulation/drivers/msp430/USB_FET_Win7_8_10_64
  

  
    CMD /C dpinst_64_eng.exe /SE /SW /SA /PATH %~dp0\ccs_base/emulation/drivers/msp430/USB_eZ-RF
  
