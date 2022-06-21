using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class GameController
{
    Tuple<int, int> size;
    int seed;
    Random random;
    public LinkedList<GameField> history;
    public int count;
    

    public GameController(Tuple<int, int> fieldsize, int rand_seed){ size = fieldsize; seed = rand_seed; random = new Random(seed); }

    public void Run(Type solver_class, int maxtime=int.MaxValue,string log_path = "", bool consoling=true)
    {
        Action<string> streamer = null;

        SolveAgent solver = (SolveAgent)Activator.CreateInstance(solver_class, new object[] { random });
        PlaceAgent placer = new B22Placer(random);
        
        if(log_path == ""){
            if(consoling){streamer = x => { Console.WriteLine(x); };}
            else{streamer = x => {};}
            Run_single(solver, placer, maxtime, streamer);
        }else{
            using(StreamWriter w = new StreamWriter(log_path,false))
            {
                if(consoling){streamer = x => { 
                    w.WriteLine(x);
                    Console.WriteLine(streamer);
                    Console.WriteLine(x); };
                }
                else{streamer = x => { w.WriteLine(x); };}

                Run_single(solver, placer, maxtime, streamer);

                w.Flush();
            }
        }

    }

    private void Run_single(SolveAgent solver, PlaceAgent placer, int maxtime, Action<string> streamer)
    {
        streamer(String.Format("solver: {0}", solver.GetType().Name));
        streamer(String.Format("seed: {0}", seed));
        streamer("-----------");

        GameField field_initial = new GameField(size);

        int n_placement = 2;
        List<Tuple<int, int>> placements = Enumerable.Range(0, size.Item1).SelectMany(row => Enumerable.Range(0, size.Item2), (first, second) => new Tuple<int, int>(first, second)).OrderBy(i => random.Next()).Take(n_placement).ToList();
        foreach(int index in Enumerable.Range(0, n_placement)) { field_initial.putNewPieceAt(placements[index], 2); }

        history = new LinkedList<GameField>();
        history.AddLast(field_initial);

         count = 1;
        
        while(count < maxtime){
            try{
                streamer(String.Format("turn: {0} player: {1}", count, solver.GetType().Name));
                streamer(history.Last.Value.ToString());

                string direction = solver.solve(history.Select(
                    x => new GameField(x)).ToList());
                streamer(String.Format(">> shift {0}", direction));
                history.AddLast(new GameField(history.Last.Value).slide(direction));
                count += 1;
                if(history.Last.Value.composed()){
                    streamer(String.Format("[Win\t] {0} won at turn {1}", solver.GetType().Name, count));
                    streamer(history.Last.Value.ToString());
                    return;
                }
            }
            catch(InvalidGameFieldSlideException){
                streamer(String.Format("[Lose\t] {0} tried to slide to invalid direction", solver.GetType().Name));
                streamer(history.Last.Value.ToString());
                return;
            }
            catch(Exception e){
                streamer("[Error\t] Unexpected error occured as below");
                streamer(history.Last.Value.ToString());
                streamer(e.Message);
                throw e;
            }
            try{
                Tuple<Tuple<int, int>, int> placement = placer.place(history.Select(x => new GameField(x)).ToList());
                GameField l = history.Last.Value;
                history.RemoveLast();
                history.AddLast(l.putNewPieceAt(placement.Item1, placement.Item2));
                if(history.Last.Value.checkmate()){
                    streamer(String.Format("[Lose\t] {0} lost at turn {1}", solver.GetType().Name, count));
                    streamer(history.Last.Value.ToString());
                    return;
                }
            }
            catch(InvalidPiecePlacementException){
                streamer(String.Format("[Lose\t] {0} tried to put on invalid place", placer.GetType().Name));
                streamer(history.Last.Value.ToString());
                return;
            }
            catch(Exception e){
                streamer("[Error\t] Unexpected error occured as below");
                streamer(history.Last.Value.ToString());
                streamer(e.Message);
                throw e;
            }
        }
        streamer("[Draw\t] time limit exceeded");
        streamer(history.Last.Value.ToString());
    }
}