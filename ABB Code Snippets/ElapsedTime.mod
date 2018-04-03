MODULE ElapsedTime
    ! below code is to calculate the elapsed time for the process 
    
    ! data decleration
    VAR clock myclock;
    VAR num myclock_reg1;
    ! procedure
    PROC elapsedTime()
        !resets the the clock
        ClkReset myclock;
        ! starts the clock before the process and stops after the process
        ClkStart myclock;
        !Process!
        ClkStop myclock;
        ! reads the clock with high resol
        myclock_reg1 := clkread (myclock \HighRes);
        ! writes the elapsed time for that process on the Teach Pendant
        TPWrite "time of this proc is ", \Num:= myclock_reg1;
        
    ENDPROC

ENDMODULE