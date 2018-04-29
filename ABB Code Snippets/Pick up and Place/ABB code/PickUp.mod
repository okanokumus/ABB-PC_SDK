MODULE PickUp
    ! (4,5,3) düzleminden parca alacak calismasi (Offset degerlerini hesaplama)
    ! x = 4, y = 5 ve z = 3
    PERS num KalanParca := 60;
    ! Step 1 : Orijin noktasini bir degiskene ata
    CONST robtarget orijin := [[37.06,135.05,146.58],[0.100968,0.104395,-0.988315,0.0462552],[-2,-4,1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];
    
    !(4,5,3)-> *(3)  *(3) *(3)  *(3) 
    !          *(3)  *(3) *(3)  *(3) 
    !          *(3)  *(3) *(3)  *(3) 
    !          *(3)  *(3) *(3)  *(3) 
    !          *(3)  *(3) *(3)  *(3) <- (1,1,1)
    
    PERS num X := 4;
    PERS num Y := 5;
    PERS num Z := 3;
    
    ! ParcaOffset
    ! MoveL ...
    ! ParcaKoordinat
    PROC  ParcaKoordinat()
        
        X := X - 1;
        IF (X = 0) THEN
            Y := Y - 1 ;
            IF (Y = 0) THEN
                Y := 5;
                Z := Z - 1;
                IF Z = 0 THEN
                    Z := 3; 
                ENDIF  
            ENDIF
            X := 4;
            KalanParca := KalanParca - 1;
            IF KalanParca < 0 THEN
                KalanParca := 0;
            ENDIF
            
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
        CASE 4: 
            OffsY := 600;
        CASE 5:
            OffsY := 800;                   
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
        DEFAULT:
            Stop;
            TPWrite "Z'in Offset degerini kontrol ediniz";
        ENDTEST          
    ENDPROC
    
ENDMODULE