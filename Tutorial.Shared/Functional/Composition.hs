-- filterSortMap1 = (map (\value -> sqrt value)) . (sortOn (\value -> value)) . (filter (\value -> value > 0))
filterSortMap1 = (map sqrt) . (sortOn id) . (filter (> 0))

-- filterSortMap2 = (filter (\value -> value > 0)) .> (sortOn (\value -> value)) .> (map (\value -> sqrt value))
filterSortMap2 = (filter (> 0)) .> (sortOn id) .> (map sqrt)