
dpi=300

table_names = ["LAI", "SW1", "SW2", "SW3", "SW4", "SW5", "SW6", "SW7",
               "GrainWt", "LeafWt", "StemWt", "PodWt", "GrainN", "LeafN", "StemN", "PodN",
               "SoilNH4_1", "SoilNH4_2", "SoilNH4_3", "SoilNH4_4",
               "SoilNH4_5", "SoilNH4_6", "SoilNH4_7",
               "SoilNO3_1", "SoilNO3_2", "SoilNO3_3", "SoilNO3_4",
               "SoilNO3_5", "SoilNO3_6", "SoilNO3_7"]

figtype = {"LAI": "wheat", "SW1": "soil", "SW2": "soil",
                "SW3": "soil", "SW4": "soil", "SW5": "soil", "SW6": "soil", "SW7": "soil",
                "GrainWt": "grain", "LeafWt": "wheat", "StemWt": "wheat", "PodWt": "wheat",
                "GrainN": "grain", "LeafN": "wheat", "StemN": "wheat", "PodN": "wheat",
                "SoilNH4_1": "soil", "SoilNH4_2": "soil", "SoilNH4_3": "soil", "SoilNH4_4": "soil",
                "SoilNH4_5": "soil", "SoilNH4_6": "soil", "SoilNH4_7": "soil",
                "SoilNO3_1": "soil", "SoilNO3_2": "soil", "SoilNO3_3": "soil", "SoilNO3_4": "soil",
                "SoilNO3_5": "soil", "SoilNO3_6": "soil", "SoilNO3_7": "soil"
                }

ylabel = {"LAI": "LAI", "SW1": "SM1", "SW2": "SM2",
            "SW3": "SM3", "SW4": "SM4", "SW5": "SM5", "SW6": "SM6", "SW7": "SM7",
            "GrainWt": "GrainWt", "LeafWt": "LeafWt", "StemWt": "StemWt", "PodWt": "PodWt",
            "GrainN": "GrainN", "LeafN": "LeafN", "StemN": "StemN", "PodN": "PodN",
            "SoilNH4_1": "NH4N1", "SoilNH4_2": "NH4N2", "SoilNH4_3": "NH4N3", "SoilNH4_4": "NH4N4",
            "SoilNH4_5": "NH4N5", "SoilNH4_6": "NH4N6", "SoilNH4_7": "NH4N7",
            "SoilNO3_1": "NO3N1", "SoilNO3_2": "NO3N2", "SoilNO3_3": "NO3N3", "SoilNO3_4": "NO3N4",
            "SoilNO3_5": "NO3N5", "SoilNO3_6": "NO3N6", "SoilNO3_7": "NO3N7"
            }

unit_AEE = {"LAI": "$m^2\ m^{-2}$", "SW1": "$m^3\ m^{-3}$", "SW2": "$m^3\ m^{-3}$",
             "SW3": "$m^3\ m^{-3}$", "SW4": "$m^3\ m^{-3}$", "SW5": "$m^3\ m^{-3}$",
             "SW6": "$m^3\ m^{-3}$", "SW7": "$m^3\ m^{-3}$",
             "GrainWt": "$g\ cm^{-2}$", "LeafWt": "$g\ cm^{-2}$", "StemWt": "$g\ cm^{-2}$",
             "PodWt": "$g\ cm^{-2}$", "GrainN": "$g\ cm^{-2}$", "LeafN": "$g\ cm^{-2}$",
             "StemN": "$g\ cm^{-2}$", "PodN": "$g\ cm^{-2}$",
             "SoilNH4_1": "$kg\ ha^{-1}$", "SoilNH4_2": "$kg\ ha^{-1}$", "SoilNH4_3": "$kg\ ha^{-1}$",
             "SoilNH4_4": "$kg\ ha^{-1}$", "SoilNH4_5": "$kg\ ha^{-1}$", "SoilNH4_6": "$kg\ ha^{-1}$",
             "SoilNH4_7": "$kg\ ha^{-1}$",
             "SoilNO3_1": "$kg\ ha^{-1}$", "SoilNO3_2": "$kg\ ha^{-1}$", "SoilNO3_3": "$kg\ ha^{-1}$",
             "SoilNO3_4": "$kg\ ha^{-1}$", "SoilNO3_5": "$kg\ ha^{-1}$", "SoilNO3_6": "$kg\ ha^{-1}$",
             "SoilNO3_7": "$kg\ ha^{-1}$"}


unit = {"LAI": "$m^2/m^2$", 'SW1': '$m^3/m^3$', 'SW2': '$m^3/m^3$',
            'SW3': '$m^3/m^3$', 'SW4': '$m^3/m^3$', 'SW5': '$m^3/m^3$', 'SW6': '$m^3/m^3$', 'SW7': '$m^3/m^3$',
             "GrainWt": "$g/cm^2$", "LeafWt": "$g/cm^2$", "StemWt": "$g/cm^2$", "PodWt": "$g/cm^2$",
             "GrainN": "$g/cm^2$", "LeafN": "$g/cm^2$", "StemN": "$g/cm^2$", "PodN": "$g/cm^2$",
             "SoilNH4_1": "$kg/ha$", "SoilNH4_2": "$kg/ha$", "SoilNH4_3": "$kg/ha$", "SoilNH4_4": "$kg/ha$",
             "SoilNH4_5": "$kg/ha$", "SoilNH4_6": "$kg/ha$", "SoilNH4_7": "$kg/ha$",
             "SoilNO3_1": "$kg/ha$", "SoilNO3_2": "$kg/ha$", "SoilNO3_3": "$kg/ha$", "SoilNO3_4": "$kg/ha$",
             "SoilNO3_5": "$kg/ha$", "SoilNO3_6": "$kg/ha$", "SoilNO3_7": "$kg/ha$"
             }

yscale = {"LAI": [0, 8.5], "SW1": [0.05, 0.55], "SW2": [0.05, 0.55],
               "SW3": [0.05, 0.55], "SW4": [0.05, 0.55], "SW5": [0.05, 0.55], "SW6": [0.05, 0.55], "SW7": [0.05, 0.55],
               "GrainWt": [0, 750], "LeafWt": [0, 450], "StemWt": [0, 1000], "PodWt": [0, 360],
               "GrainN": [0, 16], "LeafN": [0, 15], "StemN": [0, 10], "PodN": [0, 2.2],
               "SoilNH4_1": [0, 0.7], "SoilNH4_2": [0, 0.7], "SoilNH4_3": [0, 0.7], "SoilNH4_4": [0, 0.7],
               "SoilNH4_5": [0, 0.7], "SoilNH4_6": [0, 0.7], "SoilNH4_7": [0, 0.7],
               "SoilNO3_1": [0, 200], "SoilNO3_2": [0, 100], "SoilNO3_3": [0, 25], "SoilNO3_4": [0, 10],
               "SoilNO3_5": [0, 10], "SoilNO3_6": [0, 10], "SoilNO3_7": [0, 10]
               }

size_label = {"soil": (7.5, 2.5), "grain": (2.31, 2.5), "wheat": (5.04, 2.5),
              "soil_doc": (3.5, 1.5), "soil_ppt": (7.1, 1.5)}

size_no_label = {"soil_doc": (3.5, 0.5), "soil_ppt": (7.1, 0.5)}

adjust_label = {"soil": (0.1, 0.93, 0.2, 0.2, 0.8, 0.2),
                "grain": (0.3, 0.93, 0.2, 0.2, 0.8, 0.2),
                "wheat": (0.2, 0.93, 0.2, 0.2, 0.8, 0.2),
                "soil_doc": (0.1, 0.95, 0.2, 0.3, 0.7, 0.2),
                "soil_ppt": (0.1, 0.95, 0.2, 0.3, 0.7, 0.2)}


adjust_no_label = {"soil_doc": (0.1, 0.95, 0.2, 0.05, 0.95, 0.2),
                    "soil_ppt": (0.1, 0.95, 0.2, 0.05, 0.95, 0.2)}
