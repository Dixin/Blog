main = 
    do
        putStrLn (show (add 1 2))
        putStrLn (show (curriedAdd 3 4))
        putStrLn (show (addTuple (5, 6)))
        putStrLn (show (addTuple2 (7, 8)))
    where
        -- add :: Num a => a -> a -> a
        add a b = a + b
        
        -- curriedAdd :: Num a => a -> a -> a
        curriedAdd = \a -> \b -> a + b
        
        -- addTuple :: Num a => (a, a) -> a
        addTuple (a, b) = a + b
        
        -- addTuple2 :: Num a => (a, a) -> a
        addTuple2 = \t -> (fst t) + (snd t)