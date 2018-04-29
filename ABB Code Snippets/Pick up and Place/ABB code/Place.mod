MODULE Place
    ! (4,3,5) düzlemini dolduracak sekilde parça yerlestirilmesi (Offset degerlerini hesaplama)
    ! x = 4, y = 3 ve z = 5
    ! Step 1 : Orijin noktasini bir degiskene ata
    CONST robtarget orijin := [[37.06,135.05,146.58],[0.100968,0.104395,-0.988315,0.0462552],[-2,-4,1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];
    
    !          *(5)  *(5) *(5)  *(5) 
    !          *(5)  *(5) *(5)  *(5) 
    ! (1,1,1)->*(5)  *(5) *(5)  *(5) 
    
    PERS num X := 1;
    PERS num Y := 1;
    PERS num Z := 1;
    
    ! ParcaOffset
    ! MoveL ...
    ! ParcaKoordinat
    PROC  ParcaKoordinat()
        
        X := X + 1;
        IF (X MOD 5) = 0 THEN
            Y := Y + 1 ;
            IF (Y MOD 4) = 0 THEN
                Y := 1;
                Z := Z + 1;
            ENDIF
            X := 1;
            
        ENDIF
        
        ParcaOffset;
        
    ENDPROC
    
    PROC ParcaOffset()
        TEST X
        CASE 1:
            OffsX := 0;
        CASE 2: 
            OffsX := 150;
        CASE 3:
            OffsX := 300;
        CASE 4: 
            OffsX := 450;            
        DEFAULT:
            Stop;
            TPWrite "X'in Offset degerini kontrol ediniz";
        ENDTEST
        
        
        TEST Y
        CASE 1:
            OffsY := 0;
        CASE 2: 
            OffsY := 200;
        CASE 3:
            OffsY := 400;           
        DEFAULT:
            Stop;
            TPWrite "Y'in Offset degerini kontrol ediniz";
        ENDTEST  
        
        
        TEST Z
        CASE 1:
            OffsZ := 0;
        CASE 2: 
            OffsZ := 10;
        CASE 3:
            OffsZ := 20;
        CASE 4: 
            OffsZ := 30;
        CASE 5: 
            OffsZ := 40;            
        DEFAULT:
            Stop;
            TPWrite "Z'in Offset degerini kontrol ediniz";
        ENDTEST          
    ENDPROC
    
ENDMODULE