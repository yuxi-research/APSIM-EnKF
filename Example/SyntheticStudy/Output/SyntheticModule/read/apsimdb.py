# -*- coding: utf-8 -*-
"""
Created on Mon Fri 15 17:58:01 2017

@author: Yuxi Zhang
"""


class ApsimDB:
    def __init__(self, name):
        self.Name = name
        self.LayerNo = 7
        self.ID = []
        self.Clock = []
        self.DayOfYear = []
        # Plant
        self.Phenology = []
        self.ZadokStage = []
        self.Height = []
        self.CoverTotal = []
        self.Biomass = []
        self.AboveGroundWt = []
        self.AboveGroundN = []
        # Grain
        self.Yield = []
        self.GrainProtein = []
        self.GrainSize = []
        self.GrainWt = []
        self.GrainN = []
        # Leaf
        self.LAI = []
        self.LeafWt = []
        self.LeafN = []
        # Root
        self.RootDepth = []
        self.RootWt = []
        self.RootN = []
        # Pod
        self.PodWt = []
        self.PodN = []
        self.StemWt = []
        self.StemN = []
        self.GrainNO = []
        self.LAI = []
        self.RootLength = []
        self.RootDepth = []
        self.SW1 = []
        self.ST1 = []
        self.SN1 = []
        self.Phenology = []
        self.Phenology2 = []
        self.Sowing = []
        self.Germination = []
        self.Emergence = []
        self.EndOfJuvenile = []
        self.FloralInitiation = []
        self.Flowering = []
        self.StartGrainFill = []
        self.EndGrainFill = []
        self.Maturity = []
        self.HarvestRipe = []
        self.hYield = []
        self.hBiomass = []

    def read_db(self, db_file):
        import sqlite3 as lite
        import numpy as np
        con = lite.connect(db_file)
        cur = con.cursor()
        cur.execute("SELECT ID FROM Simulations WHERE Name=?", (self.Name,))
        self.ID = cur.fetchall()[0][0]

        cur.execute("SELECT [Clock.Today] FROM Report WHERE SimulationID=?", (self.ID,))
        self.Clock = np.matrix(cur.fetchall())

        cur.execute("SELECT [Clock.Today.DayOfYear] FROM Report WHERE SimulationID=?", (self.ID,))
        self.DayOfYear = np.matrix(cur.fetchall())

        # Plant
        cur.execute("SELECT [Wheat.Zadok.Stage] FROM Report WHERE SimulationID=?", (self.ID,))
        self.ZadokStage = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Stem.Height] FROM Report WHERE SimulationID=?", (self.ID,))
        self.Height = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Leaf.CoverTotal] FROM Report WHERE SimulationID=?", (self.ID,))
        self.CoverTotal = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Biomass] FROM Report WHERE SimulationID=?", (self.ID,))
        self.Biomass = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.AboveGround.Wt] FROM Report WHERE SimulationID=?", (self.ID,))
        self.AboveGroundWt = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.AboveGround.N] FROM Report WHERE SimulationID=?", (self.ID,))
        self.AboveGroundN = np.matrix(cur.fetchall())

        # Grain
        cur.execute("SELECT [Wheat.Grain.Yield] FROM Report WHERE SimulationID=?", (self.ID,))
        self.Yield = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Grain.Protein] FROM Report WHERE SimulationID=?", (self.ID,))
        self.GrainProtein = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Grain.Size] FROM Report WHERE SimulationID=?", (self.ID,))
        self.GrainSize = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Grain.Live.Wt] FROM Report WHERE SimulationID=?", (self.ID,))
        self.GrainWt = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Grain.Live.N] FROM Report WHERE SimulationID=?", (self.ID,))
        self.GrainN = np.matrix(cur.fetchall())

        # Leaf
        cur.execute("SELECT [Wheat.Leaf.LAI] FROM Report WHERE SimulationID=?", (self.ID,))
        self.LAI = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Leaf.Live.Wt] FROM Report WHERE SimulationID=?", (self.ID,))
        self.LeafWt = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Leaf.Live.N] FROM Report WHERE SimulationID=?", (self.ID,))
        self.LeafN = np.matrix(cur.fetchall())

        # Root
        cur.execute("SELECT [Clock.Today] FROM Report WHERE SimulationID=?", (self.ID,))
        self.RootDepth = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Root.Live.Wt] FROM Report WHERE SimulationID=?", (self.ID,))
        self.RootWt = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Root.Live.N] FROM Report WHERE SimulationID=?", (self.ID,))
        self.RootN = np.matrix(cur.fetchall())

        # Pod
        cur.execute("SELECT [Wheat.Pod.Live.Wt] FROM Report WHERE SimulationID=?", (self.ID,))
        self.PodWt = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Pod.Live.N] FROM Report WHERE SimulationID=?", (self.ID,))
        self.PodN = np.matrix(cur.fetchall())

        # Pod
        cur.execute("SELECT [Wheat.Stem.Live.Wt] FROM Report WHERE SimulationID=?", (self.ID,))
        self.StemWt = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Stem.Live.N] FROM Report WHERE SimulationID=?", (self.ID,))
        self.StemN = np.matrix(cur.fetchall())

        # Soil
        cur.execute("SELECT [Soil.SoilWater.SW(1)] FROM Report WHERE SimulationID=?", (self.ID,))
        self.SW1 = np.matrix(cur.fetchall())

        cur.execute("SELECT [Soil.CERESSoilTemperature.Value(1)] FROM Report WHERE SimulationID=?", (self.ID,))
        self.ST1 = np.matrix(cur.fetchall())

        cur.execute("SELECT [Soil.SoilNitrogen.NO3(1)] FROM Report WHERE SimulationID=?", (self.ID,))
        self.SN1 = np.matrix(cur.fetchall())

        # Phenology
        cur.execute("SELECT [Wheat.Phenology.CurrentStageName] FROM Report WHERE SimulationID=?", (self.ID,))
        self.Phenology = np.matrix(cur.fetchall())

        for i in range(0, len(self.Phenology)):
            self.Phenology2.append(self.Phenology[i, 0])    # Transfer phenology matrix to list

        if self.Phenology2.count("Sowing") > 0:
            self.Sowing = self.DayOfYear[self.Phenology2.index("Sowing")][0][0, 0]
        if self.Phenology2.count("Germination") > 0:
            self.Germination = self.DayOfYear[self.Phenology2.index("Germination")][0][0, 0]
        if self.Phenology2.count("Emergence") > 0:
            self.Emergence = self.DayOfYear[self.Phenology2.index("Emergence")][0][0, 0]
        if self.Phenology2.count("EndOfJuvenile") > 0:
            self.EndOfJuvenile = self.DayOfYear[self.Phenology2.index("EndOfJuvenile")][0][0, 0]
        if self.Phenology2.count("FloralInitiation") > 0:
            self.FloralInitiation = self.DayOfYear[self.Phenology2.index("FloralInitiation")][0][0, 0]
        if self.Phenology2.count("Flowering") > 0:
            self.Flowering = self.DayOfYear[self.Phenology2.index("Flowering")][0][0, 0]
        if self.Phenology2.count("StartGrainFill") > 0:
            self.StartGrainFill = self.DayOfYear[self.Phenology2.index("StartGrainFill")][0][0, 0]
        if self.Phenology2.count("EndGrainFill") > 0:
            self.EndGrainFill = self.DayOfYear[self.Phenology2.index("EndGrainFill")][0][0, 0]
        if self.Phenology2.count("Maturity") > 0:
            self.Maturity = self.DayOfYear[self.Phenology2.index("Maturity")][0][0, 0]
        if self.Phenology2.count("HarvestRipe") > 0:
            self.HarvestRipe = self.DayOfYear[self.Phenology2.index("HarvestRipe")][0][0, 0]

        self.hYield = max(self.Yield)[0]
        self.hBiomass = max(self.Biomass)[0]

    def read_phenology(self, db_file):
        import sqlite3 as lite
        import numpy as np
        con = lite.connect(db_file)
        cur = con.cursor()

        cur.execute("SELECT ID FROM Simulations WHERE Name=?", (self.Name,))
        self.ID = cur.fetchall()[0][0]

        cur.execute("SELECT [Clock.Today] FROM Report WHERE SimulationID=?", (self.ID,))
        self.Clock = np.matrix(cur.fetchall())

        cur.execute("SELECT [Clock.Today.DayOfYear] FROM Report WHERE SimulationID=?", (self.ID,))
        self.DayOfYear = np.matrix(cur.fetchall())

        cur.execute("SELECT [Wheat.Zadok.Stage] FROM Report WHERE SimulationID=?", (self.ID,))
        self.ZadokStage = np.matrix(cur.fetchall())
        self.ZadokStage2 = []
        for i in range(0, len(self.ZadokStage)):
            self.ZadokStage2.append(self.ZadokStage[i, 0])

        cur.execute("SELECT [Wheat.Phenology.CurrentStageName] FROM Report WHERE SimulationID=?", (self.ID,))
        self.Phenology = np.matrix(cur.fetchall())

        for i in range(0, len(self.Phenology)):
            self.Phenology2.append(self.Phenology[i, 0])    # Transfer phenology matrix to list

        if self.Phenology2.count("Sowing") > 0:
            self.Sowing = self.DayOfYear[self.Phenology2.index("Sowing")][0][0, 0]
        if self.Phenology2.count("Germination") > 0:
            self.Germination = self.DayOfYear[self.Phenology2.index("Germination")][0][0, 0]
        if self.Phenology2.count("Emergence") > 0:
            self.Emergence = self.DayOfYear[self.Phenology2.index("Emergence")][0][0, 0]
        if self.Phenology2.count("EndOfJuvenile") > 0:
            self.EndOfJuvenile = self.DayOfYear[self.Phenology2.index("EndOfJuvenile")][0][0, 0]
        if self.Phenology2.count("FloralInitiation") > 0:
            self.FloralInitiation = self.DayOfYear[self.Phenology2.index("FloralInitiation")][0][0, 0]
        if self.Phenology2.count("Flowering") > 0:
            self.Flowering = self.DayOfYear[self.Phenology2.index("Flowering")][0][0, 0]
        if self.Phenology2.count("StartGrainFill") > 0:
            self.StartGrainFill = self.DayOfYear[self.Phenology2.index("StartGrainFill")][0][0, 0]
        if self.Phenology2.count("EndGrainFill") > 0:
            self.EndGrainFill = self.DayOfYear[self.Phenology2.index("EndGrainFill")][0][0, 0]
        if self.Phenology2.count("Maturity") > 0:
            self.Maturity = self.DayOfYear[self.Phenology2.index("Maturity")][0][0, 0]
        if self.Phenology2.count("HarvestRipe") > 0:
            self.HarvestRipe = self.DayOfYear[self.Phenology2.index("HarvestRipe")][0][0, 0]


