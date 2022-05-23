
def format1():
    # This format create a new csv file to write statistics of APSIM DA results.
    my_file = open("Statistics.csv", 'w')  # a= append, w=write, r=read
    sep = ','
    my_file.write("states" + sep +

                  "mean_msd" + sep +
                  "mean_ensk" + sep +
                  "mean_ensp" + sep +
                  "mean_rmsd" + sep +
                  "mean_rensk" + sep +
                  "mean_rensp" + sep +

                  "truth_max" + sep +
                  "max" + sep +
                  "max_diff" + sep +
                  "ns" + sep +
                  "rmsd" + sep +
                  "mrmsd" + sep +
                  "bias" + sep +
                  "ensk_by_ensp" + sep +
                  "rensk_by_rmse" + "\n")
    my_file.close()
