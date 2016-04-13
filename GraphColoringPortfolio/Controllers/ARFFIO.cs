using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphColoringPortfolio.Controllers
{
    class ARFFIO
    {
        //######################## I/O PROCEDURES ##############################################################################################//
        public static String writeARFFHeader(String directoryPath, String learningModel, String criteria, String algorithmName)
        {
            String filePath = directoryPath;
            if (learningModel == "Regression")
                filePath += "\\" + learningModel + "_" + criteria + "_" + algorithmName + ".arff";
            else
                filePath += "\\" + learningModel + "_" + criteria + ".arff";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
            {
                if (learningModel == "Regression")
                    file.WriteLine("%1. Title: Graph Coloring Empirical Performance Model for " + algorithmName + " Coloring Algorithm");
                else
                    file.WriteLine("%1. Title: Graph Coloring Empirical Performance Model for Best Algorithm Selection");
                file.WriteLine("%");
                file.WriteLine("");
                if (learningModel == "Regression")
                    file.WriteLine("@RELATION graphcoloring_" + learningModel + "_" + criteria + "_" + algorithmName);
                else
                    file.WriteLine("@RELATION graphcoloring_" + learningModel + "_" + criteria);
                file.WriteLine("");
                using (var context = new GCPortfolioDBDataContext())
                {
                    var feature = context.ExecuteQuery<String>(@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'InstancesFeatures' AND TABLE_SCHEMA='dbo'");
                    foreach (String f in feature)
                    {
                        if (f != "Id")
                        {
                            file.Write("@ATTRIBUTE " + f);
                            if (f == "GraphID")
                                file.Write(" STRING");
                            else
                                file.Write(" NUMERIC");
                        }
                        file.WriteLine();
                    }
                }
                switch (learningModel)
                {
                    case "Regression":
                        file.WriteLine("@ATTRIBUTE class NUMERIC");
                        break;
                    case "Classification":
                        file.WriteLine("@ATTRIBUTE class {RandomGreedy, BacktrackingDSatur, DSatur, RLF, HEA, TabuCol, PartialCol, AntCol}");
                        break;
                    case "Multi-Label-Classification":
                        file.WriteLine("@ATTRIBUTE DSatur {0,1}");
                        file.WriteLine("@ATTRIBUTE BacktrackingDSatur {0,1}");
                        file.WriteLine("@ATTRIBUTE RLF {0,1}");
                        file.WriteLine("@ATTRIBUTE RandomGreedy {0,1}");
                        file.WriteLine("@ATTRIBUTE HEA {0,1}");
                        file.WriteLine("@ATTRIBUTE TabuCol {0,1}");
                        file.WriteLine("@ATTRIBUTE PartialCol {0,1}");
                        file.WriteLine("@ATTRIBUTE AntCol {0,1}");
                        break;
                }
                file.WriteLine("");
                file.WriteLine("@DATA");
            }
            return filePath;
        }

        public static void writeARFFContent(String filePath, String learningModel, String criteria, String algorithmName)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
            {
                using (var context = new GCPortfolioDBDataContext())
                {
                    foreach (InstancesFeature instance in context.InstancesFeatures)
                    {
                        GraphInstance graph = context.GraphInstances.SingleOrDefault(existingGraph => existingGraph.Id == instance.GraphID);
                        file.Write(graph.Name + ",");
                        file.Write(instance.numberofnodes.ToString() + ",");
                        file.Write(instance.numberofedges.ToString() + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.rationodesedges) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.ratioedgesnodes) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.density) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.mindegree) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.maxdegree) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.meandegree) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.stddegree) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.vcdegree) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.mediandegree) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q1degree) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q3degree) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.minbtwns) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.maxbtwns) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.meanbtwns) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.stdbtwns) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.vcbtwns) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.medianbtwns) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q1btwns) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q3btwns) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.mincloseness) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.maxcloseness) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.meancloseness) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.stdcloseness) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.vccloseness) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.mediancloseness) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q1closeness) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q3closeness) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.minegvcentrality) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.maxegvcentrality) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.meanegvcentrality) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.stdegvcentrality) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.vcegvcentrality) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.medianegvcentrality) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q1egvcentrality) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q3egvcentrality) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.mineccentricity) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.maxeccentricity) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.meaneccentricity) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.stdeccentricity) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.vceccentricity) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.medianeccentricity) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q1eccentricity) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q3eccentricity) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.minlocalclustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.maxlocalclustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.meanlocalclustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.stdlocalclustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.vclocalclustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.medianlocalclustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q1clustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q3clustering) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.minlocalwclustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.maxlocalwclustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.meanlocalwclustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.stdlocalwclustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.vclocalwclustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.medianwclustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q1wclustering) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q3wclustering) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.adjindex) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.seclargestadjegv) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.secsmallestadjegv) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.smallestadjegv) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.meanspectrum) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.stdspectrum) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.energy) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.gaplargestand2ndlargestadj) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.laplacianindex) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.seclargestlapegv) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.smallestnzlapegv) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.secondsmallestnzlapegv) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.algconnectivity) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.smallestlapegv) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.gaplargestandsmallestnzlap) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.wienerindex) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.avgpathlength) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.girth) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.degeneracy) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.connectedcomponents) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.rank) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.corank) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.szegedindex) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.beta) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.minmaxcliquen) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.maxmaxcliquen) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.meanmaxcliquen) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.vcmaxcliquen) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.medianmaxcliquen) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q1maxcliquen) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.q3maxcliquen) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.emaxcliquen) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.timemaxclique) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.maxmaxclique) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.treewidth) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.timetreewidth) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.globalclustering) + ",");

                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.ratioupbndlwbnd) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.ratiolwbndupbnd) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.distupbndlwbnd1) + ",");
                        file.Write(String.Format(CultureInfo.InvariantCulture, "{0:G}", instance.distupbndlwbnd2) + ",");

                        LabelClass classAttribute = context.LabelClasses.SingleOrDefault(existingGraph => existingGraph.GraphID == instance.GraphID);
                        switch (learningModel)
                        {
                            case "Regression":
                                if (criteria=="color")
                                    switch (algorithmName)
                                    {
                                        case "DSatur":
                                            file.WriteLine(classAttribute.RegColDSatur);
                                            break;
                                        case "BacktrackingDSatur":
                                            file.WriteLine(classAttribute.RegColBktrDSatur);
                                            break;
                                        case "RandomGreedy":
                                            file.WriteLine(classAttribute.RegColRandomGreedy);
                                            break;
                                        case "RLF":
                                            file.WriteLine(classAttribute.RegColRLF);
                                            break;
                                        case "HEA":
                                            file.WriteLine(classAttribute.RegColHEA);
                                            break;
                                        case "PartialCol":
                                            file.WriteLine(classAttribute.RegColPartialCol);
                                            break;
                                        case "TabuCol":
                                            file.WriteLine(classAttribute.RegColTabuCol);
                                            break;
                                        case "AntCol":
                                            file.WriteLine(classAttribute.RegColAntCol);
                                            break;
                                    }
                                else if(criteria=="time")
                                    switch (algorithmName)
                                    {
                                        case "DSatur":
                                            file.WriteLine(classAttribute.RegTimeDSatur);
                                            break;
                                        case "BacktrackingDSatur":
                                            file.WriteLine(classAttribute.RegTimeBktrDSatur);
                                            break;
                                        case "RandomGreedy":
                                            file.WriteLine(classAttribute.RegTimeRandomGreedy);
                                            break;
                                        case "RLF":
                                            file.WriteLine(classAttribute.RegTimeRLF);
                                            break;
                                        case "HEA":
                                            file.WriteLine(classAttribute.RegTimeHEA);
                                            break;
                                        case "PartialCol":
                                            file.WriteLine(classAttribute.RegTimePartialCol);
                                            break;
                                        case "TabuCol":
                                            file.WriteLine(classAttribute.RegTimeTabuCol);
                                            break;
                                        case "AntCol":
                                            file.WriteLine(classAttribute.RegTimeAntCol);
                                            break;
                                    }
                                break;
                            case "Classification":
                                switch (criteria)
	                            {
                                    case "ColorTime":
		                                file.WriteLine(classAttribute.LabelColorTime);
                                        break;
                                    case "ColorChecks":
                                        file.WriteLine(classAttribute.LabelColorChecks);
                                        break;
                                    case "AvgBest":
                                        file.WriteLine(classAttribute.LabelAvgBest);
                                        break;
	                            }
                                break;
                            case "Multi-Label-Classification-Zero":
                                if (classAttribute.MultiLabel0.Contains("DSatur"))
                                    file.Write(1+",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel0.Contains("BacktrackingDSatur"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel0.Contains("RLF"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel0.Contains("RandomGreedy"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel0.Contains("HEA"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel0.Contains("TabuCol"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel0.Contains("PartialCol"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel0.Contains("AntCol"))
                                    file.WriteLine(1);
                                else
                                    file.WriteLine(0);
                                break;
                            case "Multi-Label-Classification-Five":
                                if (classAttribute.MultiLabel5.Contains("DSatur"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel5.Contains("BacktrackingDSatur"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel5.Contains("RLF"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel5.Contains("RandomGreedy"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel5.Contains("HEA"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel5.Contains("TabuCol"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel5.Contains("PartialCol"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel5.Contains("AntCol"))
                                    file.WriteLine(1);
                                else
                                    file.WriteLine(0);
                                break;
                            case "Multi-Label-Classification-Ten":
                                if (classAttribute.MultiLabel10.Contains("DSatur"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel10.Contains("BacktrackingDSatur"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel10.Contains("RLF"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel10.Contains("RandomGreedy"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel10.Contains("HEA"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel10.Contains("TabuCol"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel10.Contains("PartialCol"))
                                    file.Write(1 + ",");
                                else
                                    file.Write(0 + ",");
                                if (classAttribute.MultiLabel10.Contains("AntCol"))
                                    file.WriteLine(1);
                                else
                                    file.WriteLine(0);
                                break;
                        }
                    }
                }
            }
        }

    }
}
