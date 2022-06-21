using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class GameController
{
    public void Run(Type placer_class,Type solver_class, int maxtime=int.MaxValue,string log_path = "", bool consoling=true)
    {
        Action<string> streamer = null;
        PlaceAgent placer = (PlaceAgent)Activator.CreateInstance(placer_class, new object[] { random });
        SolveAgent solver = (SolveAgent)Activator.CreateInstance(solver_class, new object[] { random });

        if(log_path == ""){
            if(consoling){streamer = x => { Console.WriteLine(x); };}
            else{streamer = x => {};}
            Run(placer, solver, maxtime, streamer);
        }else{
            using(StreamWriter w = new StreamWriter(log_path))
            {
                if(consoling){streamer = x => { w.WriteLine(x); Console.WriteLine(x); };}
                else{streamer = x => { w.WriteLine(x); };}

                Run(placer, solver, maxtime, streamer);

                w.Flush();
            }
        }

    }

    private void Run(PlaceAgent placer,SolveAgent solver, int maxtime, Action<string> streamer)
    {
        streamer(String.Format("placer: {0}", placer.GetType().Name));
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

                string direction = solver.solve(history.Select(x => new GameField(x)).ToList());
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
                streamer(String.Format("turn: {0} player: {1}", count, placer.GetType().Name));
                streamer(history.Last.Value.ToString());

                Tuple<Tuple<int, int>, int> placement = placer.place(history.Select(x => new GameField(x)).ToList());
                streamer(String.Format(">> put {1} at {0}", placement.Item1, placement.Item2));
                history.AddLast(new GameField(history.Last.Value).putNewPieceAt(placement.Item1, placement.Item2));
                count += 1;
                if(history.Last.Value.checkmate()){
                    streamer(String.Format("[Win\t] {0} won at turn {1}", placer.GetType().Name, count));
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
