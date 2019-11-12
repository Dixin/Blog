data Expression = Apply Expression Expression
                | Lambda String Expression
                | Variable String
                | S
                | K
                | I

instance Show Expression where
    show expression = toSkiString expression False

toSkiString (Apply e1 e2) parentheses = if parentheses then "(" ++ (toSkiString e1 False) ++ " " ++ (toSkiString e2 True) ++ ")" else (toSkiString e1 False) ++ " " ++ (toSkiString e2 True)
toSkiString (Variable v) parentheses = v
toSkiString S parentheses = "S"
toSkiString K parentheses = "K"
toSkiString I parentheses = "I"

toSki (Apply v e) = Apply (toSki v) (toSki e)
toSki (Lambda v e) | not $ isFree v e = Apply K (toSki e)
toSki (Lambda v1 (Variable v2)) | v1 == v2 = I
toSki (Lambda v1 (Lambda v2 e)) | isFree v1 e = toSki (Lambda v1 (toSki (Lambda v2 e)))
toSki (Lambda v (Apply e1 e2)) = Apply (Apply S (toSki $ Lambda v e1)) (toSki $ Lambda v e2)
toSki (Variable v) = Variable v
toSki S = S
toSki K = K
toSki I = I

isFree v (Apply e1 e2) = (isFree v e1) || (isFree v e2)
isFree v1 (Lambda v2 e) = v1 /= v2 && (isFree v1 e)
isFree v1 (Variable v2) = v1 == v2
isFree v S = False
isFree v K = False
isFree v I = False

createTuple = Lambda "x" $ Lambda "y" $ Lambda "f" $ (Variable "f") `Apply` (Variable "x") `Apply` (Variable "y")

main = putStrLn $ show $ toSki createTuple
