exec sp_executesql N'SELECT ((@a * @a) + (@b * @b))',N'@a float,@b float',@a=1,@b=2

exec sp_executesql N'SELECT (((@a + @b) - ((@c * @d) / 2)) + (@e * 3))',N'@a float,@b float,@c float,@d float,@e float',@a=1,@b=2,@c=3,@d=4,@e=5