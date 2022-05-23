def ol():
    import os
    import sys
    script = sys.argv[0]
    script = script.replace("/", "\\")
    path = script.split('\\control\\')

    # Add this module to path environment.
    # sys.path.append(path[0])

    # Set open-loop path.
    ol_file_1 = os.getcwd() + '\\..\\..\\OpenLoop\\States'
    ol_file_2 = os.getcwd() + '\\..\\OpenLoop\\States'
    ol_file_3 = path[0] + '\\..\\OpenLoop\\States'
    print (ol_file_3)
    ol_file = ''

    if os.path.exists(ol_file_1+'.sqlite'):
        ol_file = ol_file_1
    elif os.path.exists(ol_file_2+'.sqlite'):
        ol_file = ol_file_2
    elif os.path.exists(ol_file_3+'.sqlite'):
        ol_file = ol_file_3
    else:
        print('The open-loop file does not exist!')

    print('Open-loop file is  [' + ol_file + ']')
    print("Current directory is [" + os.getcwd() + "]")
    return ol_file
