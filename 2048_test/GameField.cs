using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class InvalidPiecePlacementException : Exception { public InvalidPiecePlacementException(string message) : base(message) { } }
class InvalidGameFieldSlideException : Exception { public InvalidGameFieldSlideException(string message) : base(message) { } }

public class GameField
{
    public readonly Tuple<int, int> size;
    private int[,] arary2d;
    public int maxpiece { get { return arary2d.Cast<int>().Max(); } }
    public int this[Tuple<int, int> position] {
        get { return arary2d[position.Item1, position.Item2]; }
        private set{arary2d[position.Item1, position.Item2] = value; }
    }

    public GameField(Tuple<int, int> fieldsize)
    {
        size = fieldsize;
        arary2d = new int[size.Item1, size.Item2];
    }
    public GameField(GameField source)
    {
        size = new Tuple<int, int>(source.size.Item1, source.size.Item2);
        arary2d = new int[size.Item1, size.Item2];
        foreach (var row in Enumerable.Range(0, size.Item1)){ foreach (var column in Enumerable.Range(0, size.Item2)) { arary2d[row, column] = source.arary2d[row, column]; } }
    }

    public override string ToString()
    {
        int maxstrsize = 4;
        string vsplit = "+";
        for (int row = 0; row < size.Item1; row++){ vsplit += new string('-', maxstrsize) + "+"; }
        string str = vsplit;
        for (int row = 0; row < size.Item1; row++)
        {
            str += "\n";
            str += "|";
            for (int column = 0; column < size.Item2; column++){ str += (arary2d[row, column] > 0 ? arary2d[row, column].ToString() : "").PadLeft(maxstrsize) + "|"; }
            str += "\n";
            str += vsplit;
        }
        return str;
    }

    public override int GetHashCode() { return arary2d.GetHashCode(); }
    public override bool Equals(object obj)
    {
        GameField fieldojb = (GameField)obj;
        return Enumerable.Range(0, size.Item1).SelectMany(row => Enumerable.Range(0, size.Item2), (first, second) => new { x = first, y = second })
            .All(pos => arary2d[pos.x, pos.y] == fieldojb.arary2d[pos.x, pos.y]);
    }
    public static bool operator ==(GameField a, GameField b){ return a.Equals(b); }
    public static bool operator !=(GameField a, GameField b){ return !a.Equals(b); }

    private static int[] shrink(int[] pices)
    {
        LinkedList<int> pices_ll = new LinkedList<int>(pices.Where(x => x != 0));
        
        LinkedListNode<int> frontier = pices_ll.First;
        while (frontier != null && frontier.Next != null)
        {
            if (frontier.Value == frontier.Next.Value)
            {
                frontier.Value *= 2;
                pices_ll.Remove(frontier.Next);
            }
            frontier = frontier.Next;
        }
        while (pices_ll.Count < pices.Length) { pices_ll.AddLast(0); }
        return pices_ll.ToArray<int>();
    }
    private static Dictionary<string, Tuple<Func<GameField, bool>, Func<GameField, GameField>>> slideactionindex = new Dictionary<string, Tuple<Func<GameField, bool>, Func<GameField, GameField>>>(){
        {"Up", new Tuple<Func<GameField, bool>, Func<GameField, GameField>>(x => x.isValidSlide("Up"), x => x.slideUp())},
        {"Down", new Tuple<Func<GameField, bool>, Func<GameField, GameField>>(x => x.isValidSlide("Down"), x => x.slideDown())},
        {"Left", new Tuple<Func<GameField, bool>, Func<GameField, GameField>>(x => x.isValidSlide("Left"), x => x.slideLeft())},
        {"Right", new Tuple<Func<GameField, bool>, Func<GameField, GameField>>(x => x.isValidSlide("Right"), x => x.slideRight())}
    };
    private GameField slideUp()
    {
        foreach (var column in Enumerable.Range(0, size.Item2))
        {
            int[] shrinked = shrink(Enumerable.Range(0, size.Item1).Select(row => arary2d[row, column]).ToArray());
            foreach (var row in Enumerable.Range(0, size.Item1)){ arary2d[row, column] = shrinked[row]; }
        }
        return this;
    }
    private GameField slideDown()
    {
        foreach (var column in Enumerable.Range(0, size.Item2))
        {
            int[] shrinked = shrink(Enumerable.Range(0, size.Item1).Select(row => arary2d[size.Item1 - row - 1, column]).ToArray());
            foreach (var row in Enumerable.Range(0, size.Item1)){ arary2d[size.Item1 - row - 1, column] = shrinked[row]; }
        }
        return this;
    }
    private GameField slideLeft()
    {
        foreach (var row in Enumerable.Range(0, size.Item1))
        {
            int[] shrinked = shrink(Enumerable.Range(0, size.Item2).Select(column => arary2d[row, column]).ToArray());
            foreach (var column in Enumerable.Range(0, size.Item2)) { arary2d[row, column] = shrinked[column]; }
        }
        return this;
    }
    private GameField slideRight()
    {
        foreach (var row in Enumerable.Range(0, size.Item1))
        {
            int[] shrinked = shrink(Enumerable.Range(0, size.Item2).Select(column => arary2d[row, size.Item2 - column - 1]).ToArray());
            foreach (var column in Enumerable.Range(0, size.Item2)){ arary2d[row, size.Item2 - column - 1] = shrinked[column]; }
        }
        return this;
    }

    public bool checkmate(){ return !isValidSlide("Up") && !isValidSlide("Down") && !isValidSlide("Left") && !isValidSlide("Right"); }
    public bool composed(){ return maxpiece >= 2048; }
    public bool isValidPosition(Tuple<int, int> position) { return 0 <= position.Item1 && position.Item1 < size.Item1 && 0 <= position.Item2 && position.Item2 < size.Item2; }
    public bool isValidPiecePlacement(Tuple<int, int> position){ return isValidPosition(position) && this[position] == 0; }
    public GameField putNewPieceAt(Tuple<int, int> position, int value=2)
    {
        if (isValidPiecePlacement(position) && (value == 2 || value == 4)){ this[position] = value; }
        else { throw new InvalidPiecePlacementException(String.Format("Invalid placement!!: ({0}, {1})", position.Item1, position.Item2)); }
        return this;
    }

    public GameField slide(string direction){
        if(slideactionindex.ContainsKey(direction)){
            var t = slideactionindex[direction];
            if(t.Item1(this)){return t.Item2(this); }
            else{ throw new InvalidGameFieldSlideException("Invalid direction: " + direction); }
        }
        else{ throw new InvalidGameFieldSlideException("Unknown direction: " + direction); }
    }
    public bool isValidSlide(string direction){
        switch(direction){
            case "Up":
            return this != new GameField(this).slideUp();
            case "Down":
            return this != new GameField(this).slideDown();
            case "Left":
            return this != new GameField(this).slideLeft();
            case "Right":
            return this != new GameField(this).slideRight();
            default:
            throw new InvalidGameFieldSlideException("Unknown direction: " + direction);
        }
    }
}
