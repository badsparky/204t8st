using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class PlaceAgent{
    protected Random rand;

    public PlaceAgent(Random random){ rand = random; }
    public abstract Tuple<Tuple<int, int>, int> place(List<GameField> history);
}

public abstract class SolveAgent{
    protected Random rand;

    public SolveAgent(Random random){ rand = random; }
    public abstract string solve(List<GameField> history); 
}
