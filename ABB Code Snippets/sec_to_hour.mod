MODULE sec_to_hour
    VAR num sec := 0;
    VAR num min := 0;
    VAR num hour := 0;
    VAR num pre_min := 0;
    ! takes total seconds and returns as (hour, min, sec)
    PROC sec_to_hour( VAR num total_sec )
        
        sec := (Round(total_sec) MOD 60);
        pre_min := Round(total_sec / 60);
        min := pre_min MOD 60;
        hour := pre_min / 60;
        
    ENDPROC

ENDMODULE