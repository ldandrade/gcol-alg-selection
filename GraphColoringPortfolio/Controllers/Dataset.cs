using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphColoringPortfolio.Controllers
{
    class Dataset
    {
        public static void setClassAttributeClassification(String criteria)
        {
            using (var context = new GCPortfolioDBDataContext())
            {
                int minimumColor;
                long minimumTime;
                long minimumChecks;
                int instanceClass = 0;
                List<String> multiLabelInstanceClass = new List<String>();
                string multiLabel;

                int avgDSatur = 0;
                int avgRLF=0;
                int avgHEA=0;
                int avgPartialCol=0;
                int avgTabuCol=0;
                int avgBacktrackingDSatur=0;
                int avgAntCol=0;
                int avgRandomGreedy=0;
                int avgBest=0;


                //For each graph instance
                foreach (GraphInstance instanceEvaluation in context.GraphInstances)
                {
                    switch (criteria)
                    {
                        case "AvgBest":
                            {
                                //List<PerformanceMetrics2> performanceMetrics = context.PerformanceMetrics2s.Where(metrics => metrics.GraphID == instanceEvaluation.Id).ToList();
                                List<PerformanceMetrics2> performanceMetrics = context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} order by Colors, Miliseconds", instanceEvaluation.Id).ToList();
                                minimumColor = performanceMetrics.ElementAt(0).Colors ?? Int16.MaxValue;
                                multiLabelInstanceClass.Clear();
                                multiLabelInstanceClass.Add(performanceMetrics.ElementAt(0).GColAlgorithm);
                                //Sweep PerformanceMetrics table in search for best algorithms that reached minimum coloring
                                for (int gcolalgindex = 1; gcolalgindex < performanceMetrics.Count; gcolalgindex++)
                                {
                                    if (performanceMetrics.ElementAt(gcolalgindex).Colors < minimumColor)
                                    {
                                        minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                        multiLabelInstanceClass.Clear();
                                        multiLabelInstanceClass.Add(performanceMetrics.ElementAt(gcolalgindex).GColAlgorithm);
                                    }
                                    else if (performanceMetrics.ElementAt(gcolalgindex).Colors == minimumColor)
                                        multiLabelInstanceClass.Add(performanceMetrics.ElementAt(gcolalgindex).GColAlgorithm);
                                }
                                multiLabel = String.Empty;
                                multiLabelInstanceClass = multiLabelInstanceClass.Select(x => x).Distinct().ToList();
                                for (int index = 0; index < multiLabelInstanceClass.Count; index++)
                                    multiLabel = String.Concat(multiLabel, ",", multiLabelInstanceClass.ElementAt(index));
                                if (multiLabel.Contains("DSatur"))
                                    avgDSatur++;
                                if (multiLabel.Contains("RLF"))
                                    avgRLF++;
                                if (multiLabel.Contains("RandomGreedy"))
                                    avgRandomGreedy++;
                                if (multiLabel.Contains("BacktrackingDSatur"))
                                    avgBacktrackingDSatur++;
                                if (multiLabel.Contains("HEA"))
                                    avgHEA++;
                                if (multiLabel.Contains("TabuCol"))
                                    avgTabuCol++;
                                if (multiLabel.Contains("PartialCol"))
                                    avgPartialCol++;
                                if (multiLabel.Contains("AntCol"))
                                    avgAntCol++;
                            }
                            break;
                        case "ColorTime":
                            {
                                //List<PerformanceMetrics2> performanceMetrics = context.PerformanceMetrics2s.Where(metrics => metrics.GraphID == instanceEvaluation.Id).ToList();
                                List<PerformanceMetrics2> performanceMetrics = context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} order by Colors, Miliseconds", instanceEvaluation.Id).ToList();
                                minimumColor = performanceMetrics.ElementAt(0).Colors ?? Int16.MaxValue;
                                minimumTime = performanceMetrics.ElementAt(0).Miliseconds ?? Int64.MaxValue;
                                instanceClass = 0;

                                for (int gcolalgindex = 1; gcolalgindex < performanceMetrics.Count; gcolalgindex++)
                                {
                                    if (performanceMetrics.ElementAt(gcolalgindex).Colors < minimumColor)
                                    {
                                        minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                        minimumTime = performanceMetrics.ElementAt(gcolalgindex).Miliseconds ?? Int64.MaxValue;
                                        instanceClass = gcolalgindex;
                                    }
                                    else if (performanceMetrics.ElementAt(gcolalgindex).Colors == minimumColor)
                                        if (performanceMetrics.ElementAt(gcolalgindex).Miliseconds < minimumTime)
                                        {
                                            minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                            minimumTime = performanceMetrics.ElementAt(gcolalgindex).Miliseconds ?? Int64.MaxValue;
                                            instanceClass = gcolalgindex;
                                        }
                                }
                                LabelClass label = context.LabelClasses.SingleOrDefault(instanceLabel => instanceLabel.GraphID == instanceEvaluation.Id);
                                if (label == null)
                                {
                                    label = new LabelClass();
                                    label.Id = Guid.NewGuid();
                                    label.GraphID = instanceEvaluation.Id;
                                    context.LabelClasses.InsertOnSubmit(label);
                                }
                                label.LabelColorTime = performanceMetrics.ElementAt(instanceClass).GColAlgorithm;
                                context.SubmitChanges();
                            }
                            break;
                        case "ColorChecks":
                            {
                                //List<PerformanceMetrics2> performanceMetrics = context.PerformanceMetrics2s.Where(metrics => metrics.GraphID == instanceEvaluation.Id).ToList();
                                List<PerformanceMetrics2> performanceMetrics = context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} order by Colors, Checks", instanceEvaluation.Id).ToList();
                                minimumColor = performanceMetrics.ElementAt(0).Colors ?? Int16.MaxValue;
                                minimumChecks = performanceMetrics.ElementAt(0).Checks ?? Int64.MaxValue;
                                instanceClass = 0;

                                for (int gcolalgindex = 1; gcolalgindex < performanceMetrics.Count; gcolalgindex++)
                                {
                                    if (performanceMetrics.ElementAt(gcolalgindex).Colors < minimumColor)
                                    {
                                        minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                        minimumChecks = performanceMetrics.ElementAt(gcolalgindex).Checks ?? Int64.MaxValue;
                                        instanceClass = gcolalgindex;
                                    }
                                    else if (performanceMetrics.ElementAt(gcolalgindex).Colors == minimumColor)
                                        if (performanceMetrics.ElementAt(gcolalgindex).Checks < minimumChecks)
                                        {
                                            minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                            minimumChecks = performanceMetrics.ElementAt(gcolalgindex).Checks ?? Int64.MaxValue;
                                            instanceClass = gcolalgindex;
                                        }
                                }
                                LabelClass label = context.LabelClasses.SingleOrDefault(instanceLabel => instanceLabel.GraphID == instanceEvaluation.Id);
                                if (label == null)
                                {
                                    label = new LabelClass();
                                    label.Id = Guid.NewGuid();
                                    label.GraphID = instanceEvaluation.Id;
                                    context.LabelClasses.InsertOnSubmit(label);
                                }
                                label.LabelColorChecks = performanceMetrics.ElementAt(instanceClass).GColAlgorithm;
                                context.SubmitChanges();
                            }
                            break;
                        case "MultiLabel0":
                            {
                                //List<PerformanceMetrics2> performanceMetrics = context.PerformanceMetrics2s.Where(metrics => metrics.GraphID == instanceEvaluation.Id).ToList();
                                List<PerformanceMetrics2> performanceMetrics = context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} order by Colors, Miliseconds", instanceEvaluation.Id).ToList();
                                minimumColor = performanceMetrics.ElementAt(0).Colors ?? Int16.MaxValue;
                                multiLabelInstanceClass.Clear();
                                multiLabelInstanceClass.Add(performanceMetrics.ElementAt(0).GColAlgorithm);
                                for (int gcolalgindex = 1; gcolalgindex < performanceMetrics.Count; gcolalgindex++)
                                {
                                    if (performanceMetrics.ElementAt(gcolalgindex).Colors < minimumColor)
                                    {
                                        minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                        multiLabelInstanceClass.Clear();
                                        multiLabelInstanceClass.Add(performanceMetrics.ElementAt(gcolalgindex).GColAlgorithm);
                                    }
                                    else if (performanceMetrics.ElementAt(gcolalgindex).Colors == minimumColor)
                                        multiLabelInstanceClass.Add(performanceMetrics.ElementAt(gcolalgindex).GColAlgorithm);
                                }
                                LabelClass label = context.LabelClasses.SingleOrDefault(instanceLabel => instanceLabel.GraphID == instanceEvaluation.Id);
                                if (label == null)
                                {
                                    label = new LabelClass();
                                    label.Id = Guid.NewGuid();
                                    label.GraphID = instanceEvaluation.Id;
                                    context.LabelClasses.InsertOnSubmit(label);
                                }
                                multiLabel = String.Empty;
                                multiLabelInstanceClass=multiLabelInstanceClass.Select(x => x).Distinct().ToList();
                                for (int index = 0; index < multiLabelInstanceClass.Count; index++)
                                    multiLabel = String.Concat(multiLabel, ",", multiLabelInstanceClass.ElementAt(index));
                                label.MultiLabel0 = multiLabel;
                                context.SubmitChanges();
                            }
                            break;
                        case "MultiLabel5":
                            {
                                //List<PerformanceMetrics2> performanceMetrics = context.PerformanceMetrics2s.Where(metrics => metrics.GraphID == instanceEvaluation.Id).ToList();
                                List<PerformanceMetrics2> performanceMetrics = context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} order by Colors, Miliseconds", instanceEvaluation.Id).ToList();
                                minimumColor = performanceMetrics.ElementAt(0).Colors ?? Int16.MaxValue;
                                multiLabelInstanceClass.Clear();
                                multiLabelInstanceClass.Add(performanceMetrics.ElementAt(0).GColAlgorithm);
                                for (int gcolalgindex = 1; gcolalgindex < performanceMetrics.Count; gcolalgindex++)
                                {
                                    if (performanceMetrics.ElementAt(gcolalgindex).Colors < minimumColor)
                                    {
                                        minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                        multiLabelInstanceClass.Clear();
                                        multiLabelInstanceClass.Add(performanceMetrics.ElementAt(gcolalgindex).GColAlgorithm);
                                    }
                                    else if (performanceMetrics.ElementAt(gcolalgindex).Colors <= minimumColor*1.05)
                                        multiLabelInstanceClass.Add(performanceMetrics.ElementAt(gcolalgindex).GColAlgorithm);
                                }
                                LabelClass label = context.LabelClasses.SingleOrDefault(instanceLabel => instanceLabel.GraphID == instanceEvaluation.Id);
                                if (label == null)
                                {
                                    label = new LabelClass();
                                    label.Id = Guid.NewGuid();
                                    label.GraphID = instanceEvaluation.Id;
                                    context.LabelClasses.InsertOnSubmit(label);
                                }
                                multiLabel = String.Empty;
                                multiLabelInstanceClass = multiLabelInstanceClass.Select(x => x).Distinct().ToList();
                                for (int index = 0; index < multiLabelInstanceClass.Count; index++)
                                    multiLabel = String.Concat(multiLabel, ",", multiLabelInstanceClass.ElementAt(index));
                                label.MultiLabel5 = multiLabel;
                                context.SubmitChanges();
                            }
                            break;
                        case "MultiLabel10":
                            {
                                //List<PerformanceMetrics2> performanceMetrics = context.PerformanceMetrics2s.Where(metrics => metrics.GraphID == instanceEvaluation.Id).ToList();
                                List<PerformanceMetrics2> performanceMetrics = context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} order by Colors, Miliseconds", instanceEvaluation.Id).ToList();
                                minimumColor = performanceMetrics.ElementAt(0).Colors ?? Int16.MaxValue;
                                multiLabelInstanceClass.Clear();
                                multiLabelInstanceClass.Add(performanceMetrics.ElementAt(0).GColAlgorithm);
                                for (int gcolalgindex = 1; gcolalgindex < performanceMetrics.Count; gcolalgindex++)
                                {
                                    if (performanceMetrics.ElementAt(gcolalgindex).Colors < minimumColor)
                                    {
                                        minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                        multiLabelInstanceClass.Clear();
                                        multiLabelInstanceClass.Add(performanceMetrics.ElementAt(gcolalgindex).GColAlgorithm);
                                    }
                                    else if (performanceMetrics.ElementAt(gcolalgindex).Colors <= minimumColor*1.1)
                                        multiLabelInstanceClass.Add(performanceMetrics.ElementAt(gcolalgindex).GColAlgorithm);
                                }
                                LabelClass label = context.LabelClasses.SingleOrDefault(instanceLabel => instanceLabel.GraphID == instanceEvaluation.Id);
                                if (label == null)
                                {
                                    label = new LabelClass();
                                    label.Id = Guid.NewGuid();
                                    label.GraphID = instanceEvaluation.Id;
                                    context.LabelClasses.InsertOnSubmit(label);
                                }
                                multiLabel = String.Empty;
                                multiLabelInstanceClass = multiLabelInstanceClass.Select(x => x).Distinct().ToList();
                                for (int index = 0; index < multiLabelInstanceClass.Count; index++)
                                    multiLabel = String.Concat(multiLabel, ",", multiLabelInstanceClass.ElementAt(index));
                                label.MultiLabel10 = multiLabel;
                                context.SubmitChanges();
                            }
                            break;
                    }
                }
                if (criteria=="AvgBest")
                    foreach (GraphInstance instanceEvaluation in context.GraphInstances)
                    {
                        avgBest = 0;
                        List<PerformanceMetrics2> performanceMetrics = context.PerformanceMetrics2s.Where(metrics => metrics.GraphID == instanceEvaluation.Id).ToList();
                        minimumColor = performanceMetrics.ElementAt(0).Colors ?? Int16.MaxValue;
                        instanceClass = 0;
                        for (int gcolalgindex = 1; gcolalgindex < performanceMetrics.Count; gcolalgindex++)
                        {
                            if (performanceMetrics.ElementAt(gcolalgindex).Colors < minimumColor)
                            {
                                minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                instanceClass = gcolalgindex;
                                switch (performanceMetrics.ElementAt(gcolalgindex).GColAlgorithm)
                                {
                                    case "DSatur":
                                        avgBest = avgDSatur;
                                        break;
                                    case "RLF":
                                        avgBest = avgRLF;
                                        break;
                                    case "RandomGreedy":
                                        avgBest = avgRandomGreedy;
                                        break;
                                    case "HEA":
                                        avgBest = avgHEA;
                                        break;
                                    case "TabuCol":
                                        avgBest = avgTabuCol;
                                        break;
                                    case "PartialCol":
                                        avgBest = avgPartialCol;
                                        break;
                                    case "AntCol":
                                        avgBest = avgAntCol;
                                        break;
                                    case "BacktrackingDSatur":
                                        avgBest = avgBacktrackingDSatur;
                                        break;
                                }
                            }
                            else if (performanceMetrics.ElementAt(gcolalgindex).Colors == minimumColor)
                                switch (performanceMetrics.ElementAt(gcolalgindex).GColAlgorithm)
                                {
                                    case "DSatur":
                                        if (avgDSatur >= avgBest)
                                        {
                                            avgBest = avgDSatur;
                                            minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                            instanceClass = gcolalgindex;
                                        }
                                        break;
                                    case "RLF":
                                        if (avgRLF >= avgBest)
                                        {
                                            avgBest = avgRLF;
                                            minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                            instanceClass = gcolalgindex;
                                        }
                                        break;
                                    case "RandomGreedy":
                                        if (avgRandomGreedy >= avgBest)
                                        {
                                            avgBest = avgRandomGreedy;
                                            minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                            instanceClass = gcolalgindex;
                                        }
                                        break;
                                    case "HEA":
                                        if (avgHEA >= avgBest)
                                        {
                                            avgBest = avgHEA;
                                            minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                            instanceClass = gcolalgindex;
                                        }
                                        break;
                                    case "TabuCol":
                                        if (avgTabuCol >= avgBest)
                                        {
                                            avgBest = avgTabuCol;
                                            minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                            instanceClass = gcolalgindex;
                                        }
                                        break;
                                    case "PartialCol":
                                        if (avgPartialCol >= avgBest)
                                        {
                                            avgBest = avgPartialCol;
                                            minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                            instanceClass = gcolalgindex;
                                        }
                                        break;
                                    case "AntCol":
                                        if (avgAntCol >= avgBest)
                                        {
                                            avgBest = avgAntCol;
                                            minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                            instanceClass = gcolalgindex;
                                        }
                                        break;
                                    case "BacktrackingDSatur":
                                        if (avgBacktrackingDSatur >= avgBest)
                                        {
                                            avgBest = avgBacktrackingDSatur;
                                            minimumColor = performanceMetrics.ElementAt(gcolalgindex).Colors ?? Int16.MaxValue;
                                            instanceClass = gcolalgindex;
                                        }
                                        break;
                                }
                        }
                        LabelClass label = context.LabelClasses.SingleOrDefault(instanceLabel => instanceLabel.GraphID == instanceEvaluation.Id);
                        if (label == null)
                        {
                            label = new LabelClass();
                            label.Id = Guid.NewGuid();
                            label.GraphID = instanceEvaluation.Id;
                            context.LabelClasses.InsertOnSubmit(label);
                        }
                        label.LabelAvgBest = performanceMetrics.ElementAt(instanceClass).GColAlgorithm;
                        context.SubmitChanges();
                }
            }
        }

        public static void setClassAttributeRegression(String criteria)
        {
            using (var context = new GCPortfolioDBDataContext())
            {
                foreach (GraphInstance instanceEvaluation in context.GraphInstances)
                {
                    if (criteria == "color")
                    {
                        Int16 RegColDSatur = (Int16)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='DSatur' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Colors;
                        Int16 RegColBktrDSatur = (Int16)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='BacktrackingDSatur' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Colors;
                        Int16 RegColRLF = (Int16)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='RLF' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Colors;
                        Int16 RegColRandomGreedy = (Int16)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='RandomGreedy' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Colors;
                        Int16 RegColHEA = (Int16)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='HEA' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Colors;
                        Int16 RegColPartialCol = (Int16)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='PartialCol' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Colors;
                        Int16 RegColTabuCol = (Int16)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='TabuCol' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Colors;
                        Int16 RegColAntCol = (Int16)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='AntCol' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Colors;

                        LabelClass label = context.LabelClasses.SingleOrDefault(instanceLabel => instanceLabel.GraphID == instanceEvaluation.Id);
                        if (label == null)
                        {
                            label = new LabelClass();
                            label.Id = Guid.NewGuid();
                            label.GraphID = instanceEvaluation.Id;
                            context.LabelClasses.InsertOnSubmit(label);
                        }
                        label.RegColDSatur = RegColDSatur;
                        label.RegColBktrDSatur = RegColBktrDSatur;
                        label.RegColRLF = RegColRLF;
                        label.RegColRandomGreedy = RegColRandomGreedy;
                        label.RegColHEA = RegColHEA;
                        label.RegColPartialCol = RegColPartialCol;
                        label.RegColTabuCol = RegColTabuCol;
                        label.RegColAntCol = RegColAntCol;
                        context.SubmitChanges();
                    }
                    else if (criteria == "time")
                    {
                        Int32 RegTimeDSatur = (Int32)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='DSatur' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Miliseconds;
                        Int32 RegTimeBktrDSatur = (Int32)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='BacktrackingDSatur' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Miliseconds;
                        Int32 RegTimeRLF = (Int32)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='RLF' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Miliseconds;
                        Int32 RegTimeRandomGreedy = (Int32)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='RandomGreedy' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Miliseconds;
                        Int32 RegTimeHEA = (Int32)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='HEA' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Miliseconds;
                        Int32 RegTimePartialCol = (Int32)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='PartialCol' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Miliseconds;
                        Int32 RegTimeTabuCol = (Int32)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='TabuCol' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Miliseconds;
                        Int32 RegTimeAntCol = (Int32)context.ExecuteQuery<PerformanceMetrics2>(@"select * from PerformanceMetrics2 where GraphID={0} and GColAlgorithm='AntCol' order by Colors ASC", instanceEvaluation.Id).ToList().FirstOrDefault().Miliseconds;

                        LabelClass label = context.LabelClasses.SingleOrDefault(instanceLabel => instanceLabel.GraphID == instanceEvaluation.Id);
                        if (label == null)
                        {
                            label = new LabelClass();
                            label.Id = Guid.NewGuid();
                            label.GraphID = instanceEvaluation.Id;
                            context.LabelClasses.InsertOnSubmit(label);
                        }
                        label.RegTimeDSatur = RegTimeDSatur;
                        label.RegTimeBktrDSatur = RegTimeBktrDSatur;
                        label.RegTimeRLF = RegTimeRLF;
                        label.RegTimeRandomGreedy = RegTimeRandomGreedy;
                        label.RegTimeHEA = RegTimeHEA;
                        label.RegTimePartialCol = RegTimePartialCol;
                        label.RegTimeTabuCol = RegTimeTabuCol;
                        label.RegTimeAntCol = RegTimeAntCol;
                        context.SubmitChanges();
                    }
                }
            }
        }
    }
}