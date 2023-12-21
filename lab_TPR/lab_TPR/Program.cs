class Program
{
    static double[] probabilities;
    static double[][] matrix;
    static void Main()
    {
        
        ReadFromFile("C:\\TPR\\matrix1.txt");
        Console.WriteLine("Ймовiрностi:");
        foreach (var probability in probabilities)
        {
            Console.Write($"{probability} ");
        }
        Console.WriteLine();

        Console.WriteLine("Матриця:");
        foreach (var row in matrix)
        {
            foreach (var value in row)
            {
                Console.Write($"{value} ");
            }
            Console.WriteLine();
        }
        
        Console.WriteLine("1. В умовах ризику\n" +
                          "2. В умовах невизначеностi");
        int number = Convert.ToInt32(Console.ReadLine());
        switch (number)
        {  
           case 1 :
               ConditionsOfRisk();
               break;
           case 2 :
               ConditionsOfUncertainty();
               break;
           default:
               Console.WriteLine("unknown");
               break;
        }
    }
    static void ReadFromFile(string filePath)
    {
        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line = sr.ReadLine();

                if (line != null && line.Trim().ToLower() == "probabilities")
                {
                    line = sr.ReadLine();

                    if (line != null)
                    {
                        string[] probabilityStrings = line.Split(' ');
                        probabilities = Array.ConvertAll(probabilityStrings, double.Parse);

                        double sumOfProbabilities = probabilities.Sum();
                        if (Math.Abs(sumOfProbabilities - 1.0) > 0.000001)
                        {
                            Console.WriteLine("Сума ймовiрностей не дорiвнює 1.");
                        }
                    }
                    var matrixList = new List<double[]>();
                    line = sr.ReadLine();
                    while (!sr.EndOfStream)
                    {
                        
                        string matrixLine = sr.ReadLine();
                        if (matrixLine != null)
                        {
                            string[] matrixValues = matrixLine.Split(',');
                            double[] matrixRow = Array.ConvertAll(matrixValues, double.Parse);
                            matrixList.Add(matrixRow);
                        }
                    }
                    matrix = matrixList.ToArray();
                }
                else if (line != null && line.Trim().ToLower() == "matrix")
                {
                    var matrixList = new List<double[]>();

                    while (!sr.EndOfStream)
                    {
                        string matrixLine = sr.ReadLine();
                        if (matrixLine != null)
                        {
                            string[] matrixValues = matrixLine.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            double[] matrixRow = Array.ConvertAll(matrixValues, double.Parse);
                            matrixList.Add(matrixRow);
                        }
                    }

                    if (matrixList.Count > 0)
                    {
                        matrix = matrixList.ToArray();
                        // Leave probabilities array empty
                        probabilities = Array.Empty<double>();
                    }
                    else
                    {
                        Console.WriteLine("Файл містить невірний формат або відсутні дані.");
                    }
                }
                else
                {
                    Console.WriteLine("Файл містить невірний формат або відсутні дані.");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }
    
    
    static void ConditionsOfRisk()
    {
        var result1 = Bayess(probabilities, matrix);
        MinDisp(probabilities, matrix, result1);
        MaxRozp(probabilities, matrix);
        Modal(probabilities, matrix);
    }

    static void ConditionsOfUncertainty()
    {
        Maximaxa(matrix);
        Minimaxa(matrix);
        Hyrviz(matrix);
        Sevidzh(matrix);
    }

    static double[] Bayess(double[] p, double[][] matrix)
    {
        double maxProduct = double.MinValue;
        List<int> variants = new List<int>();
        double[] resultArray = new double[matrix.GetLength(0)];

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            double product = 0;

            for (int j = 0; j < matrix[0].Length; j++)
            {
                product += matrix[i][j] * p[j];
            }

            resultArray[i] = product;
            if (product > maxProduct)
            {
                maxProduct = product;
                variants.Clear(); 
                variants.Add(i + 1); 
            }
            else if (Math.Abs(product - maxProduct) < double.Epsilon)
            {
                variants.Add(i + 1);
            }
        }

        Console.WriteLine($"Критерiй Байесса:");
        foreach (var variant in variants)
        {
            Console.WriteLine($"z{variant}");
        }

        if (variants.Count > 1)
        {
            int variant = FindBestVariant(matrix, variants);
            Console.WriteLine($"Найоптимальнiший варiант: z{variant}");
        }
        return resultArray;
    }

    static void MinDisp(double[] p, double[][] matrix, double[] bayess)
    {
        double minProduct = double.MaxValue;
        List<int> variants = new List<int>();

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            double product = 0;

            for (int j = 0; j < matrix[0].Length; j++)
            {
                product += matrix[i][j]  * matrix[i][j] * p[j];
            }
            product -= bayess[i] * bayess[i];
            if (Math.Sqrt(product) < minProduct)
            {
                minProduct = Math.Sqrt(product);
                variants.Clear(); 
                variants.Add(i + 1);
            }
            else if (Math.Abs(Math.Sqrt(product) - minProduct) < double.Epsilon)
            {
                variants.Add(i + 1);
            }
        }
        Console.WriteLine($"Критерiй мiнiмiзацiї дисперсiї:");
        foreach (var variant in variants)
        {
            Console.WriteLine($"z{variant}");
        }
        if (variants.Count > 1)
        {
            int variant = FindBestVariant(matrix, variants);
            Console.WriteLine($"Найоптимальнiший варiант: z{variant}");
        }
    }

    static void MaxRozp(double[] p, double[][] matrix)
    {
        double maxInMatrix = double.MinValue;
        double minInMatrix = double.MaxValue;

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 1; j < matrix[0].Length; j++)
            {
                if (matrix[i][j] > maxInMatrix)
                {
                    maxInMatrix = matrix[i][j];
                }

                if (matrix[i][j] < minInMatrix)
                {
                    minInMatrix = matrix[i][j];
                }
            }
        }
        Console.WriteLine($"Введiть допустиме значення A в межах [{minInMatrix};{maxInMatrix}]");
        var a = Convert.ToDouble(Console.ReadLine());
        List<int> variants = new List<int>();
        double[] resultArray = new double[matrix.GetLength(0)];
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix[0].Length; j++)
            {
                if (matrix[i][j] >= a)
                {
                    resultArray[i] += p[j];
                }
            }

            if (Math.Abs(resultArray[i] - resultArray[variants.FirstOrDefault()]) < double.Epsilon)
            {
                variants.Add(i + 1);
            }
            else if (resultArray[i] > resultArray[variants.FirstOrDefault()])
            {
                variants.Clear();
                variants.Add(i + 1);
            }
        }
        Console.WriteLine($"Критерiй максимiзацiї ймовiрностей розподiлу:");
        foreach (var variant in variants)
        {
            Console.WriteLine($"z{variant}");
        }
        if (variants.Count > 1)
        {
            int variant = FindBestVariant(matrix, variants);
            Console.WriteLine($"Найоптимальнiший варiант: z{variant}");
        }
    }

    static void Modal(double[] p, double[][] matrix)
    {
        double maxP = p.Max();
        double position = p.ToList().IndexOf(maxP);
        var countMaxP = 0;
        for (int i = 0; i < p.GetLength(0); i++)
        {
            if (p[i] == maxP)
            {
                countMaxP += 1;
            }
        }

        if (countMaxP > 1)
        {
            Console.WriteLine("Модальний критерiй\n Неможливо використати модальний критерiй, оскiльки у нас є 2 максимальнi ймовiрностi");
        }
        else
        {
            double maxNumber = double.MinValue;
            List<int> variants = new List<int>();

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                if (matrix[i][(int)position] > maxNumber)
                {
                    maxNumber = matrix[i][(int)position];
                    variants.Clear();
                    variants.Add(i + 1);
                }
                else if (Math.Abs(matrix[i][(int)position] - maxNumber) < double.Epsilon)
                {
                    variants.Add(i + 1);
                }
            }
            Console.WriteLine($"Модальний критерiй:"); 
            foreach (var variant in variants)
            {
                Console.WriteLine($"z{variant}");
            }
            if (variants.Count > 1)
            {
                int variant = FindBestVariant(matrix, variants);
                Console.WriteLine($"Найоптимальнiший варiант: z{variant}");
            }
        }
    }

    static void Maximaxa(double[][] matrix)
    {
        double maxZ = double.MinValue;
        List<int> variants = new List<int>();

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            double maxInRow = matrix[i][0];

            for (int j = 1; j < matrix[0].Length; j++)
            {
                if (matrix[i][j] > maxInRow)
                {
                    maxInRow = matrix[i][j];
                }
            }

            if (maxInRow > maxZ)
            {
                maxZ = maxInRow;
                variants.Clear();
                variants.Add(i + 1); 
            }
            else if (Math.Abs(maxInRow - maxZ) < double.Epsilon)
            {
                variants.Add(i + 1);
            }
        }
        Console.WriteLine($"Критерiй максимакса:");
        foreach (var variant in variants)
        {
            Console.WriteLine($"z{variant}");
        }
        if (variants.Count > 1)
        {
            int variant = FindBestVariant(matrix, variants);
            Console.WriteLine($"Найоптимальнiший варiант: z{variant}");
        }
    }

    static void Minimaxa(double[][] matrix)
    {
        Console.WriteLine("Оберiть тип даних, якi ви ввели:\n" +
                          "1. Прибутки\n" +
                          "2. Збитки");
        int a = Convert.ToInt32(Console.ReadLine());
        List<int> variants = new List<int>();
        switch (a)
        {
            case 1:
                double maxZ = double.MinValue;
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    double minInRow = matrix[i][0];

                    for (int j = 1; j < matrix[0].Length; j++)
                    {
                        if (matrix[i][j] < minInRow)
                        {
                            minInRow = matrix[i][j];
                        }
                    }

                    if (minInRow > maxZ)
                    {
                        maxZ = minInRow;
                        variants.Clear(); 
                        variants.Add(i + 1); 
                    }
                    else if (Math.Abs(minInRow - maxZ) < double.Epsilon)
                    {
                        variants.Add(i + 1);
                    }
                }
                break;
            case 2:
                double minZ = double.MaxValue;
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    double maxInRow = matrix[i][0];

                    for (int j = 1; j < matrix[0].Length; j++)
                    {
                        if (matrix[i][j] > maxInRow)
                        {
                            maxInRow = matrix[i][j];
                        }
                    }

                    if (maxInRow < minZ)
                    {
                        minZ = maxInRow;
                        variants.Clear(); 
                        variants.Add(i + 1); 
                    }
                    else if (Math.Abs(maxInRow - minZ) < double.Epsilon)
                    {
                        variants.Add(i + 1);
                    }
                }   
                break;
            default:
                Console.WriteLine("unknown");
                break; 
        }
        Console.WriteLine($"Мiнiмаксний критерiй:");
        foreach (var variant in variants)
        {
            Console.WriteLine($"z{variant}");
        }
        if (variants.Count > 1)
        {
            int variant = FindBestVariant(matrix, variants);
            Console.WriteLine($"Найоптимальнiший варiант: z{variant}");
        }
    }

    static void Hyrviz(double[][] matrix)
    {
        while (true)
        {
            Console.WriteLine("Введiть коефiцiєнт ваги (-1 для завершення):");
            double alpha = Convert.ToDouble(Console.ReadLine());

            if (alpha == -1)
            {
                break; 
            }

            List<int> variants = new List<int>();
            double maxZ = double.MinValue;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                double maxInRow = double.MinValue;
                double minInRow = double.MaxValue;

                for (int j = 1; j < matrix[0].Length; j++)
                {
                    if (matrix[i][j] > maxInRow)
                    {
                        maxInRow = matrix[i][j];
                    }

                    if (matrix[i][j] < minInRow)
                    {
                        minInRow = matrix[i][j];
                    }
                }

                double zi = (alpha * maxInRow + (1 - alpha) * minInRow);

                if (zi > maxZ)
                {
                    maxZ = zi;
                    variants.Clear();
                    variants.Add(i + 1);
                }
                else if (Math.Abs(zi - maxZ) < double.Epsilon)
                {
                    variants.Add(i + 1);
                }
            }

            Console.WriteLine($"Критерiй Гурвiца:");
            foreach (var variant in variants)
            {
                Console.WriteLine($"z{variant}, Z={maxZ}");
            }
            if (variants.Count > 1)
            {
                int variant = FindBestVariant(matrix, variants);
                Console.WriteLine($"Найоптимальнiший варiант: z{variant}");
            }
        }
    }

    static void Sevidzh(double[][] matrix)
    {
        double[] maxInColumn = new double[matrix[0].Length];
        List<int> variants = new List<int>();

        for (int j = 0; j < matrix[0].Length; j++)
        {
            double maxInCol = matrix[0][j];

            for (int i = 1; i < matrix.GetLength(0); i++)
            {
                if (matrix[i][j] > maxInCol)
                {
                    maxInCol = matrix[i][j];
                }
            }

            maxInColumn[j] = maxInCol;
        }
        double[,] newMatrix = new double[matrix.GetLength(0), matrix[0].Length];

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix[0].Length; j++)
            {
                newMatrix[i, j] = maxInColumn[j] - matrix[i][j];
            }
        }
        
        double minOfMaxInRow = double.MaxValue;

        for (int i = 0; i < newMatrix.GetLength(0); i++)
        {
            double maxInRow = newMatrix[i, 0];

            for (int j = 1; j < newMatrix.GetLength(1); j++)
            {
                if (newMatrix[i, j] > maxInRow)
                {
                    maxInRow = newMatrix[i, j];
                }
            }

            if (maxInRow < minOfMaxInRow)
            {
                minOfMaxInRow = maxInRow;
                variants.Clear();
                variants.Add(i + 1); 
            }
            else if (Math.Abs(maxInRow - minOfMaxInRow) < double.Epsilon)
            {
                variants.Add(i + 1);
            }
        }
        Console.WriteLine($"Критерiй Севiджа:");
        foreach (var variant in variants)
        {
            Console.WriteLine($"z{variant}");
        }
        if (variants.Count > 1)
        {
            int variant = FindBestVariant(matrix, variants);
            Console.WriteLine($"Найоптимальнiший варiант: z{variant}");
        }
    }
    
    static int FindBestVariant(double[][] matrix, List<int> variants)
    {
        int bestVariant = -1;
        double minNegativeSum = double.MinValue;
        double maxRowSum = double.MinValue;

        foreach (var currentVariant in variants)
        {
            int row = currentVariant - 1;
            double negativeSum = 0;
            double rowSum = 0;

            for (int j = 1; j < matrix[0].Length; j++)
            {
                if (matrix[row][j] < 0)
                {
                    negativeSum += matrix[row][j];
                }

                rowSum += matrix[row][j];
            }

            if (negativeSum > minNegativeSum || (negativeSum == minNegativeSum && rowSum > maxRowSum))
            {
                minNegativeSum = negativeSum;
                maxRowSum = rowSum;
                bestVariant = currentVariant;
            }
        }

        return bestVariant;
    }
}