using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Drawing;
using System.IO;
namespace curseuch
{
    #region 1. Модели данных результатов и циклов наблюдений

    public sealed class MeasurementCycle
    {
        public string Label { get; }
        public Dictionary<string, double> PointValues { get; }
        public MeasurementCycle(string label, Dictionary<string, double> pointValues)
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            PointValues = pointValues ?? throw new ArgumentNullException(nameof(pointValues));
        }
    }
    public sealed class PhaseStateItem
    {
        public string Label { get; set; } = string.Empty;
        public double M { get; set; }
        public double MPlusE { get; set; }
        public double MMinusE { get; set; }
        public double A { get; set; }
        public double APlusE { get; set; }
        public double AMinusE { get; set; }
        public double R { get; set; }
        public double L { get; set; }
        public string StatusText { get; set; } = string.Empty;
    }
    public sealed class Level1AnalysisResult
    {
        public List<PhaseStateItem> Items { get; } = new List<PhaseStateItem>();
        public double ForecastM { get; set; }
        public double ForecastMPlusE { get; set; }
        public double ForecastMMinusE { get; set; }
        public double ForecastA { get; set; }
        public double ForecastAPlusE { get; set; }
        public double ForecastAMinusE { get; set; }
        public double ForecastR { get; set; }
        public double ForecastL { get; set; }
        public string ForecastStatusText { get; set; } = string.Empty;
        public double InputEpsilon { get; set; }
        public double CheckEpsilon { get; set; }
        public double Alpha { get; set; }
    }
    public sealed class Level2AnalysisResult
    {
        public string BlockName { get; set; } = string.Empty;
        public List<PhaseStateItem> Items { get; } = new List<PhaseStateItem>();
        public double ForecastM { get; set; }
        public double ForecastMPlusE { get; set; }
        public double ForecastMMinusE { get; set; }
        public double ForecastA { get; set; }
        public double ForecastAPlusE { get; set; }
        public double ForecastAMinusE { get; set; }
        public double ForecastR { get; set; }
        public double ForecastL { get; set; }
        public string ForecastStatusText { get; set; } = string.Empty;
        public double InputEpsilon { get; set; }
        public double CheckEpsilon { get; set; }
        public double Alpha { get; set; }
    }
    public sealed class Level3ResponseResult
    {
        public string BlockName { get; set; } = string.Empty;
        public int ClusterId { get; set; }
        public List<PhaseStateItem> Items { get; } = new List<PhaseStateItem>();
        public double ForecastM { get; set; }
        public double ForecastMPlusE { get; set; }
        public double ForecastMMinusE { get; set; }
        public double ForecastA { get; set; }
        public double ForecastAPlusE { get; set; }
        public double ForecastAMinusE { get; set; }
        public double ForecastR { get; set; }
        public double ForecastL { get; set; }
        public string ForecastStatusText { get; set; } = string.Empty;
        public double InputEpsilon { get; set; }
        public double CheckEpsilon { get; set; }
        public double Alpha { get; set; }
    }
    public sealed class Level4PointStateItem
    {
        public string CycleLabel { get; set; } = string.Empty;
        public string PointName { get; set; } = string.Empty;
        public double Z { get; set; }
        public double ZPlusE { get; set; }
        public double ZMinusE { get; set; }
        public double R { get; set; }
        public double L { get; set; }
        public string StatusText { get; set; } = string.Empty;
    }
    public sealed class Level4PointForecastItem
    {
        public string PointName { get; set; } = string.Empty;
        public double ForecastZ { get; set; }
        public double ForecastZPlusE { get; set; }
        public double ForecastZMinusE { get; set; }
        public double R { get; set; }
        public double L { get; set; }
        public string ForecastStatusText { get; set; } = string.Empty;
    }
    public sealed class Level4AnalysisResult
    {
        public string PointName { get; set; } = string.Empty;
        public List<string> PointNames { get; } = new List<string>();
        public List<Level4PointStateItem> Items { get; } = new List<Level4PointStateItem>();
        public List<Level4PointForecastItem> Forecasts { get; } = new List<Level4PointForecastItem>();
        public double ForecastZ { get; set; }
        public double ForecastZPlusE { get; set; }
        public double ForecastZMinusE { get; set; }
        public double ForecastDelta { get; set; }
        public double ForecastVelocity { get; set; }
        public double ForecastAcceleration { get; set; }
        public string ForecastStatusText { get; set; } = string.Empty;
    }
    #endregion

    #region 2. Модели и сервисы III уровня

    public enum ClusteringMethodType
    {
        Hierarchical
    }
    public sealed class ClusterFeature
    {
        public string PointName { get; set; } = string.Empty;
        public double Mean { get; set; }
        public double Variance { get; set; }
        public double Range { get; set; }
        public double MeanVelocity { get; set; }
        public double NormalizedMean { get; set; }
        public double NormalizedVariance { get; set; }
        public double NormalizedRange { get; set; }
        public double NormalizedMeanVelocity { get; set; }
        public double[] ToVector()
        {
            return new[]
            {
                NormalizedMean,
                NormalizedVariance,
                NormalizedRange,
                NormalizedMeanVelocity
                };
        }
    }
    public sealed class ClusterAssignment
    {
        public string PointName { get; set; } = string.Empty;
        public int ClusterId { get; set; }
        public bool IsNoise { get; set; }
    }
    public sealed class ClusterAnalysisResult
    {
        public string BlockName { get; set; } = string.Empty;
        public ClusteringMethodType MethodType { get; set; }
        public List<ClusterFeature> Features { get; } = new List<ClusterFeature>();
        public List<ClusterAssignment> Assignments { get; } = new List<ClusterAssignment>();
        public int RecommendedClusterId { get; set; } = -1;
    }
    public sealed class RigidLink
    {
        public string PointA { get; set; } = string.Empty;
        public string PointB { get; set; } = string.Empty;
        public double ReferenceDistance { get; set; }
        public double MaxDeviation { get; set; }
        public bool IsRigid { get; set; }
    }
    public sealed class StructuralCluster
    {
        public int ClusterId { get; set; }
        public List<string> PointNames { get; } = new List<string>();
        public bool IsIsolatedPoint { get; set; }
    }
    public sealed class Level3StructuralResult
    {
        public string BlockName { get; set; } = string.Empty;
        public ClusteringMethodType MethodType { get; set; }
        public List<RigidLink> Links { get; } = new List<RigidLink>();
        public List<StructuralCluster> Clusters { get; } = new List<StructuralCluster>();
        public List<string> StandalonePoints { get; } = new List<string>();
        public int RecommendedClusterId { get; set; } = -1;
    }
    public interface IClusteringMethod
    {
        ClusterAnalysisResult Cluster(
            string blockName,
            List<ClusterFeature> features);
    }
    public sealed class HierarchicalClusteringService : IClusteringMethod
    {
        public ClusterAnalysisResult Cluster(string blockName, List<ClusterFeature> features)
        {
            if (features == null || features.Count == 0)
                throw new ArgumentException("Нет признаков для кластеризации.");
            ClusterAnalysisResult result = new ClusterAnalysisResult
            {
                BlockName = blockName,
                MethodType = ClusteringMethodType.Hierarchical
            };
            foreach (var feature in features)
                result.Features.Add(feature);
            if (features.Count == 1)
            {
                result.Assignments.Add(new ClusterAssignment
                {
                    PointName = features[0].PointName,
                    ClusterId = 0,
                    IsNoise = false
                });
                result.RecommendedClusterId = 0;
                return result;
            }
            double globalScore = features.Average(f => f.NormalizedVariance + f.NormalizedRange + Math.Abs(f.NormalizedMeanVelocity));
            foreach (var feature in features)
            {
                double score = feature.NormalizedVariance + feature.NormalizedRange + Math.Abs(feature.NormalizedMeanVelocity);
                int clusterId = score <= globalScore ? 0 : 1;
                result.Assignments.Add(new ClusterAssignment
                {
                    PointName = feature.PointName,
                    ClusterId = clusterId,
                    IsNoise = false
                });
            }
            result.RecommendedClusterId = SelectMostSuspiciousCluster(result, features);
            return result;
        }
        private static int SelectMostSuspiciousCluster(ClusterAnalysisResult result, List<ClusterFeature> features)
        {
            var grouped = result.Assignments
                .GroupBy(a => a.ClusterId)
                .Select(g => new
                {
                    ClusterId = g.Key,
                    Score = g.Join(features,
                        a => a.PointName,
                        f => f.PointName,
                        (a, f) => f.Variance + f.Range + Math.Abs(f.MeanVelocity))
                        .Average()
                })
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();
            return grouped?.ClusterId ?? 0;
        }
    }
    #endregion

    public sealed class curseuch
    {
        private readonly Random _random = new Random();

        #region 3. Работа с SQLite, фото и метаданными объекта

        private static SQLiteConnection CreateConnection(string databasePath)
        {
            return new SQLiteConnection($"Data Source={databasePath};Version=3;");
        }
        public List<string> GetTableNames(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                throw new ArgumentException("Не указан путь к базе данных.");
            List<string> tableNames = new List<string>();
            using (SQLiteConnection connection = CreateConnection(databasePath))
            {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = Convert.ToString(reader["name"]);
                        if (!string.IsNullOrWhiteSpace(name))
                            tableNames.Add(name);
                    }
                }
            }
            return tableNames;
        }
        public DataTable LoadTable(string databasePath, string tableName)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                throw new ArgumentException("Не указан путь к базе данных.");
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Не указано имя таблицы.");
            DataTable table = new DataTable();
            using (SQLiteConnection connection = CreateConnection(databasePath))
            {
                connection.Open();
                string query = $"SELECT * FROM \"{tableName}\";";
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection))
                {
                    adapter.Fill(table);
                }
            }
            return table;
        }
        public void SaveTable(string databasePath, string tableName, DataTable table)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                throw new ArgumentException("Не указан путь к базе данных.");
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Не указано имя таблицы.");
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            using (SQLiteConnection connection = CreateConnection(databasePath))
            {
                connection.Open();
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    using (SQLiteCommand deleteCommand = new SQLiteCommand($"DELETE FROM \"{tableName}\";", connection, transaction))
                    {
                        deleteCommand.ExecuteNonQuery();
                    }
                    List<string> columnNames = table.Columns
                        .Cast<DataColumn>()
                        .Select(c => $"\"{c.ColumnName}\"")
                        .ToList();
                    List<string> parameterNames = table.Columns
                        .Cast<DataColumn>()
                        .Select((c, i) => $"@p{i}")
                        .ToList();
                    string insertSql = $"INSERT INTO \"{tableName}\" ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", parameterNames)});";
                    foreach (DataRow row in table.Rows)
                    {
                        if (row.RowState == DataRowState.Deleted)
                            continue;
                        using (SQLiteCommand insertCommand = new SQLiteCommand(insertSql, connection, transaction))
                        {
                            for (int i = 0; i < table.Columns.Count; i++)
                            {
                                object value = row[i];
                                if (value == null || value == DBNull.Value)
                                {
                                    insertCommand.Parameters.AddWithValue(parameterNames[i], DBNull.Value);
                                }
                                else
                                {
                                    if (value is double d)
                                        insertCommand.Parameters.AddWithValue(parameterNames[i], d);
                                    else if (value is float f)
                                        insertCommand.Parameters.AddWithValue(parameterNames[i], f);
                                    else if (value is decimal m)
                                        insertCommand.Parameters.AddWithValue(parameterNames[i], m);
                                    else
                                        insertCommand.Parameters.AddWithValue(parameterNames[i], value);
                                }
                            }
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
        }
        public void DeleteRow(DataTable table, int rowIndex)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (rowIndex < 0 || rowIndex >= table.Rows.Count)
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            table.Rows.RemoveAt(rowIndex);
        }
        public Image LoadImageFromDatabase(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                return null;
            using (SQLiteConnection connection = CreateConnection(databasePath))
            {
                connection.Open();
                string query = "SELECT * FROM \"photo\" LIMIT 1;";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        object value = reader.GetValue(i);
                        if (value == null || value == DBNull.Value)
                            continue;
                        if (value is byte[] bytes && bytes.Length > 0)
                        {
                            using (MemoryStream ms = new MemoryStream(bytes))
                            using (Image temp = Image.FromStream(ms))
                            {
                                return new Bitmap(temp);
                            }
                        }
                    }
                }
            }
            return null;
        }
        public void SaveImageToDatabase(string databasePath, string imagePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                throw new ArgumentException("Не указан путь к базе данных.");
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                throw new FileNotFoundException("Файл схемы не найден.", imagePath);
            byte[] bytes = File.ReadAllBytes(imagePath);
            using (SQLiteConnection connection = CreateConnection(databasePath))
            {
                connection.Open();
                EnsurePhotoTableAndMetadata(connection);
                using (SQLiteCommand countCommand = new SQLiteCommand("SELECT COUNT(*) FROM \"photo\";", connection))
                {
                    long count = (long)countCommand.ExecuteScalar();
                    if (count == 0)
                    {
                        using (SQLiteCommand insertCommand = new SQLiteCommand("INSERT INTO \"photo\" (\"photo\") VALUES (@photo);", connection))
                        {
                            insertCommand.Parameters.AddWithValue("@photo", bytes);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (SQLiteCommand updateCommand = new SQLiteCommand("UPDATE \"photo\" SET \"photo\"=@photo WHERE rowid=(SELECT rowid FROM \"photo\" LIMIT 1);", connection))
                        {
                            updateCommand.Parameters.AddWithValue("@photo", bytes);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
        public void SaveObjectMetadata(string databasePath, int blocksCount, double epsilon, double alpha)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                return;
            using (SQLiteConnection connection = CreateConnection(databasePath))
            {
                connection.Open();
                EnsurePhotoTableAndMetadata(connection);
                using (SQLiteCommand countCommand = new SQLiteCommand("SELECT COUNT(*) FROM \"photo\";", connection))
                {
                    long count = (long)countCommand.ExecuteScalar();
                    if (count == 0)
                    {
                        using (SQLiteCommand insertCommand = new SQLiteCommand("INSERT INTO \"photo\" (\"blocks_count\", \"epsilon_e\", \"alpha_a\", \"E\", \"A\") VALUES (@b, @e, @a, @e, @a);", connection))
                        {
                            insertCommand.Parameters.AddWithValue("@b", blocksCount);
                            insertCommand.Parameters.AddWithValue("@e", epsilon);
                            insertCommand.Parameters.AddWithValue("@a", alpha);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (SQLiteCommand updateCommand = new SQLiteCommand("UPDATE \"photo\" SET \"blocks_count\"=@b, \"epsilon_e\"=@e, \"alpha_a\"=@a, \"E\"=@e, \"A\"=@a WHERE rowid=(SELECT rowid FROM \"photo\" LIMIT 1);", connection))
                        {
                            updateCommand.Parameters.AddWithValue("@b", blocksCount);
                            updateCommand.Parameters.AddWithValue("@e", epsilon);
                            updateCommand.Parameters.AddWithValue("@a", alpha);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
        public bool TryLoadObjectMetadata(string databasePath, out int blocksCount, out double epsilon, out double alpha)
        {
            blocksCount = 0;
            epsilon = 0.0;
            alpha = 0.0;
            if (string.IsNullOrWhiteSpace(databasePath))
                return false;
            using (SQLiteConnection connection = CreateConnection(databasePath))
            {
                connection.Open();
                EnsurePhotoTableAndMetadata(connection);
                using (SQLiteCommand command = new SQLiteCommand("SELECT \"blocks_count\", \"epsilon_e\", \"alpha_a\", \"E\", \"A\" FROM \"photo\" LIMIT 1;", connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return false;
                    blocksCount = reader["blocks_count"] == DBNull.Value ? 0 : Convert.ToInt32(reader["blocks_count"]);
                    object epsilonValue = reader["E"] != DBNull.Value ? reader["E"] : reader["epsilon_e"];
                    object alphaValue = reader["A"] != DBNull.Value ? reader["A"] : reader["alpha_a"];

                    epsilon = epsilonValue == DBNull.Value ? 0.0 : Convert.ToDouble(epsilonValue, CultureInfo.InvariantCulture);
                    alpha = alphaValue == DBNull.Value ? 0.0 : Convert.ToDouble(alphaValue, CultureInfo.InvariantCulture);
                    return true;
                }
            }
        }
        private void EnsurePhotoTableAndMetadata(SQLiteConnection connection)
        {
            using (SQLiteCommand createCommand = new SQLiteCommand("CREATE TABLE IF NOT EXISTS \"photo\" (\"photo\" BLOB);", connection))
                createCommand.ExecuteNonQuery();
            List<string> columns = GetColumnNames(connection, "photo");
            if (!columns.Contains("blocks_count"))
            {
                using (SQLiteCommand command = new SQLiteCommand("ALTER TABLE \"photo\" ADD COLUMN \"blocks_count\" INTEGER;", connection))
                    command.ExecuteNonQuery();
            }
            if (!columns.Contains("epsilon_e"))
            {
                using (SQLiteCommand command = new SQLiteCommand("ALTER TABLE \"photo\" ADD COLUMN \"epsilon_e\" REAL;", connection))
                    command.ExecuteNonQuery();
            }
            if (!columns.Contains("alpha_a"))
            {
                using (SQLiteCommand command = new SQLiteCommand("ALTER TABLE \"photo\" ADD COLUMN \"alpha_a\" REAL;", connection))
                    command.ExecuteNonQuery();
            }
            if (!columns.Contains("E"))
            {
                using (SQLiteCommand command = new SQLiteCommand("ALTER TABLE \"photo\" ADD COLUMN \"E\" REAL;", connection))
                    command.ExecuteNonQuery();
            }
            if (!columns.Contains("A"))
            {
                using (SQLiteCommand command = new SQLiteCommand("ALTER TABLE \"photo\" ADD COLUMN \"A\" REAL;", connection))
                    command.ExecuteNonQuery();
            }
        }
        private List<string> GetColumnNames(SQLiteConnection connection, string tableName)
        {
            List<string> columns = new List<string>();
            string query = $"PRAGMA table_info(\"{tableName}\");";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string columnName = Convert.ToString(reader["name"]);
                    if (!string.IsNullOrWhiteSpace(columnName))
                        columns.Add(columnName);
                }
            }
            return columns;
        }
        #endregion

        #region 4. Ввод данных, генерация строк и преобразование таблицы

        public void AddEmptyRow(DataTable table)
        {
            AddGeneratedRow(table);
        }
        public void AddGeneratedRow(DataTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            DataRow newRow = table.NewRow();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                DataColumn column = table.Columns[i];
                if (i == 0)
                {
                    newRow[column.ColumnName] = GetNextRowLabel(table, column.ColumnName);
                    continue;
                }
                if (IsNumericColumn(column))
                {
                    newRow[column.ColumnName] = ForecastNextColumnValue(table, column);
                }
                else
                {
                    newRow[column.ColumnName] = DBNull.Value;
                }
            }
            table.Rows.Add(newRow);
        }
        private string GetNextRowLabel(DataTable table, string firstColumnName)
        {
            int maxValue = -1;
            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                    continue;
                string text = Convert.ToString(row[firstColumnName]);
                if (int.TryParse(text, out int value))
                {
                    if (value > maxValue)
                        maxValue = value;
                }
            }
            return (maxValue + 1).ToString();
        }
        private object ForecastNextColumnValue(DataTable table, DataColumn column)
        {
            List<double> values = new List<double>();
            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (TryReadDouble(row[column], out double value))
                    values.Add(value);
            }

            if (values.Count == 0)
                return ConvertGeneratedValue(column, 0.0);

            double last = values[values.Count - 1];
            if (values.Count == 1)
                return ConvertGeneratedValue(column, last);

            double sumAdjacentDifferences = 0.0;
            for (int k = 0; k < values.Count - 1; k++)
                sumAdjacentDifferences += Math.Abs(values[k + 1] - values[k]);

            double d = sumAdjacentDifferences / (values.Count - 1);
            double randomDelta = (2.0 * _random.NextDouble() - 1.0) * d;
            double generated = last + randomDelta;

            return ConvertGeneratedValue(column, generated);
        }
        private static object ConvertGeneratedValue(DataColumn column, double value)
        {
            if (column.DataType == typeof(int) || column.DataType == typeof(long) || column.DataType == typeof(short))
                return Convert.ChangeType(Math.Round(value), column.DataType, CultureInfo.InvariantCulture);

            double rounded = Math.Round(value, 4);

            if (column.DataType == typeof(float))
                return (float)rounded;
            if (column.DataType == typeof(decimal))
                return Convert.ToDecimal(rounded, CultureInfo.InvariantCulture);
            if (column.DataType == typeof(string))
                return rounded.ToString("0.####", CultureInfo.InvariantCulture);

            return rounded;
        }
        private static bool IsNumericColumn(DataColumn column)
        {
            return column.DataType == typeof(double)
                || column.DataType == typeof(float)
                || column.DataType == typeof(decimal)
                || column.DataType == typeof(int)
                || column.DataType == typeof(long)
                || column.DataType == typeof(short);
        }
        private static bool TryReadDouble(object value, out double result)
        {
            result = 0.0;
            if (value == null || value == DBNull.Value)
                return false;
            if (value is double d)
            {
                result = d;
                return true;
            }
            if (value is float f)
            {
                result = f;
                return true;
            }
            if (value is decimal m)
            {
                result = (double)m;
                return true;
            }
            if (value is int i)
            {
                result = i;
                return true;
            }
            if (value is long l)
            {
                result = l;
                return true;
            }
            if (value is short sh)
            {
                result = sh;
                return true;
            }
            string text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(text))
                return false;
            text = text.Replace(',', '.');
            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }
        public List<MeasurementCycle> ConvertTableToMeasurementCycles(DataTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (table.Columns.Count < 2)
                throw new InvalidOperationException("Недостаточно столбцов для построения циклов наблюдений.");
            string labelColumnName = table.Columns[0].ColumnName;
            List<DataColumn> numericColumns = new List<DataColumn>();
            for (int i = 1; i < table.Columns.Count; i++)
            {
                numericColumns.Add(table.Columns[i]);
            }
            List<MeasurementCycle> cycles = new List<MeasurementCycle>();
            foreach (DataRow row in table.Rows)
            {
                Dictionary<string, double> pointValues = new Dictionary<string, double>();
                foreach (DataColumn column in numericColumns)
                {
                    object rawValue = row[column];
                    if (rawValue == DBNull.Value)
                        continue;
                    if (!TryConvertToDouble(rawValue, out double value))
                        continue;
                    pointValues[column.ColumnName] = value;
                }
                if (pointValues.Count > 0)
                {
                    string label = Convert.ToString(row[labelColumnName]);
                    if (string.IsNullOrWhiteSpace(label))
                        label = cycles.Count.ToString();
                    cycles.Add(new MeasurementCycle(label, pointValues));
                }
            }
            if (cycles.Count == 0)
                throw new InvalidOperationException("Не удалось извлечь числовые данные контрольных точек.");
            return cycles;
        }
        private static bool TryConvertToDouble(object rawValue, out double value)
        {
            value = 0.0;
            if (rawValue == null || rawValue == DBNull.Value)
                return false;
            if (rawValue is double d)
            {
                value = d;
                return true;
            }
            if (rawValue is float f)
            {
                value = f;
                return true;
            }
            if (rawValue is decimal m)
            {
                value = (double)m;
                return true;
            }
            string text = Convert.ToString(rawValue)?.Replace(',', '.');
            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }
        #endregion

        #region 5. Математический аппарат I уровня декомпозиции

        public Level1AnalysisResult RunLevel1(List<MeasurementCycle> cycles, double epsilon, double alpha)
        {
            if (cycles == null || cycles.Count == 0)
                throw new ArgumentException("Нет циклов наблюдений для анализа.");
            if (epsilon <= 0)
                throw new ArgumentOutOfRangeException(nameof(epsilon), "E должно быть больше 0.");
            if (alpha <= 0 || alpha >= 1)
                throw new ArgumentOutOfRangeException(nameof(alpha), "A должно принадлежать интервалу (0;1).");
            double epsilonCheck = epsilon;
            List<string> pointNames = cycles[0].PointValues.Keys.ToList();
            if (pointNames.Count == 0)
                throw new InvalidOperationException("Не найдены контрольные точки.");
            List<double> baseVector = ExtractVector(cycles[0], pointNames);
            List<double> baseVectorPlus = baseVector.Select(v => v + epsilonCheck).ToList();
            List<double> baseVectorMinus = baseVector.Select(v => v - epsilonCheck).ToList();
            double baseM = CalculateNorm(baseVector);
            Level1AnalysisResult result = new Level1AnalysisResult
            {
                InputEpsilon = epsilon,
                CheckEpsilon = epsilonCheck,
                Alpha = alpha
            };
            foreach (MeasurementCycle cycle in cycles)
            {
                List<double> currentVector = ExtractVector(cycle, pointNames);
                List<double> currentVectorPlus = currentVector.Select(v => v + epsilonCheck).ToList();
                List<double> currentVectorMinus = currentVector.Select(v => v - epsilonCheck).ToList();
                double m = CalculateNorm(currentVector);
                double mPlus = CalculateNorm(currentVectorPlus);
                double mMinus = CalculateNorm(currentVectorMinus);
                double a = CalculateAngle(baseVector, currentVector);
                double aPlus = CalculateAngle(baseVectorPlus, currentVectorPlus);
                double aMinus = CalculateAngle(baseVectorMinus, currentVectorMinus);
                double r = Math.Abs(mPlus - mMinus) / 2.0;
                double l = Math.Abs(m - baseM);
                result.Items.Add(new PhaseStateItem
                {
                    Label = cycle.Label,
                    M = RoundValue(m),
                    MPlusE = RoundValue(mPlus),
                    MMinusE = RoundValue(mMinus),
                    A = RoundValue(a),
                    APlusE = RoundValue(aPlus),
                    AMinusE = RoundValue(aMinus),
                    R = RoundValue(r),
                    L = RoundValue(l),
                    StatusText = GetStatusText(r, l)
                });
            }
            FillForecast(result, alpha, baseM);
            return result;
        }
        private static void FillForecast(Level1AnalysisResult result, double alpha, double baseM)
        {
            List<double> mValues = result.Items.Select(x => x.M).ToList();
            List<double> mPlusValues = result.Items.Select(x => x.MPlusE).ToList();
            List<double> mMinusValues = result.Items.Select(x => x.MMinusE).ToList();
            List<double> aValues = result.Items.Select(x => x.A).ToList();
            List<double> aPlusValues = result.Items.Select(x => x.APlusE).ToList();
            List<double> aMinusValues = result.Items.Select(x => x.AMinusE).ToList();
            result.ForecastM = RoundValue(ExponentialForecast(mValues, alpha));
            result.ForecastMPlusE = RoundValue(ExponentialForecast(mPlusValues, alpha));
            result.ForecastMMinusE = RoundValue(ExponentialForecast(mMinusValues, alpha));
            result.ForecastA = RoundValue(ExponentialForecast(aValues, alpha));
            result.ForecastAPlusE = RoundValue(ExponentialForecast(aPlusValues, alpha));
            result.ForecastAMinusE = RoundValue(ExponentialForecast(aMinusValues, alpha));
            result.ForecastR = RoundValue(Math.Abs(result.ForecastMPlusE - result.ForecastMMinusE) / 2.0);
            result.ForecastL = RoundValue(Math.Abs(result.ForecastM - baseM));
            result.ForecastStatusText = GetStatusText(result.ForecastR, result.ForecastL);
        }
        #endregion

        #region 6. Математический аппарат II уровня декомпозиции

        public Level2AnalysisResult RunLevel2(
    List<MeasurementCycle> cycles,
    List<string> selectedColumns,
    double epsilon,
    double alpha,
    string blockName)
        {
            if (cycles == null || cycles.Count == 0)
                throw new ArgumentException("Нет циклов наблюдений для анализа.");
            if (selectedColumns == null || selectedColumns.Count < 2)
                throw new ArgumentException("Для расчёта блока нужно минимум 2 контрольные точки.");
            if (epsilon <= 0)
                throw new ArgumentOutOfRangeException(nameof(epsilon), "E должно быть больше 0.");
            if (alpha <= 0 || alpha >= 1)
                throw new ArgumentOutOfRangeException(nameof(alpha), "A должно принадлежать интервалу (0;1).");
            double epsilonCheck = epsilon;
            List<double> baseVector = ExtractVector(cycles[0], selectedColumns);
            List<double> baseVectorPlus = baseVector.Select(v => v + epsilonCheck).ToList();
            List<double> baseVectorMinus = baseVector.Select(v => v - epsilonCheck).ToList();
            double baseM = CalculateNorm(baseVector);
            Level2AnalysisResult result = new Level2AnalysisResult
            {
                BlockName = blockName,
                InputEpsilon = epsilon,
                CheckEpsilon = epsilonCheck,
                Alpha = alpha
            };
            foreach (MeasurementCycle cycle in cycles)
            {
                List<double> currentVector = ExtractVector(cycle, selectedColumns);
                List<double> currentVectorPlus = currentVector.Select(v => v + epsilonCheck).ToList();
                List<double> currentVectorMinus = currentVector.Select(v => v - epsilonCheck).ToList();
                double m = CalculateNorm(currentVector);
                double mPlus = CalculateNorm(currentVectorPlus);
                double mMinus = CalculateNorm(currentVectorMinus);
                double a = CalculateAngle(baseVector, currentVector);
                double aPlus = CalculateAngle(baseVectorPlus, currentVectorPlus);
                double aMinus = CalculateAngle(baseVectorMinus, currentVectorMinus);
                double r = Math.Abs(mPlus - mMinus) / 2.0;
                double l = Math.Abs(m - baseM);
                result.Items.Add(new PhaseStateItem
                {
                    Label = cycle.Label,
                    M = RoundValue(m),
                    MPlusE = RoundValue(mPlus),
                    MMinusE = RoundValue(mMinus),
                    A = RoundValue(a),
                    APlusE = RoundValue(aPlus),
                    AMinusE = RoundValue(aMinus),
                    R = RoundValue(r),
                    L = RoundValue(l),
                    StatusText = GetStatusText(r, l)
                });
            }
            FillForecastLevel2(result, alpha, baseM);
            return result;
        }
        private static void FillForecastLevel2(Level2AnalysisResult result, double alpha, double baseM)
        {
            List<double> mValues = result.Items.Select(x => x.M).ToList();
            List<double> mPlusValues = result.Items.Select(x => x.MPlusE).ToList();
            List<double> mMinusValues = result.Items.Select(x => x.MMinusE).ToList();
            List<double> aValues = result.Items.Select(x => x.A).ToList();
            List<double> aPlusValues = result.Items.Select(x => x.APlusE).ToList();
            List<double> aMinusValues = result.Items.Select(x => x.AMinusE).ToList();
            result.ForecastM = RoundValue(ExponentialForecast(mValues, alpha));
            result.ForecastMPlusE = RoundValue(ExponentialForecast(mPlusValues, alpha));
            result.ForecastMMinusE = RoundValue(ExponentialForecast(mMinusValues, alpha));
            result.ForecastA = RoundValue(ExponentialForecast(aValues, alpha));
            result.ForecastAPlusE = RoundValue(ExponentialForecast(aPlusValues, alpha));
            result.ForecastAMinusE = RoundValue(ExponentialForecast(aMinusValues, alpha));
            result.ForecastR = RoundValue(Math.Abs(result.ForecastMPlusE - result.ForecastMMinusE) / 2.0);
            result.ForecastL = RoundValue(Math.Abs(result.ForecastM - baseM));
            result.ForecastStatusText = GetStatusText(result.ForecastR, result.ForecastL);
        }
        #endregion

        #region 7. Математический аппарат III уровня декомпозиции

        public Level3StructuralResult RunLevel3Structural(
    List<MeasurementCycle> cycles,
    List<string> selectedColumns,
    string blockName,
    ClusteringMethodType methodType,
    double rigidTolerance)
        {
            if (cycles == null || cycles.Count == 0)
                throw new ArgumentException("Нет циклов наблюдений для анализа.");
            if (selectedColumns == null || selectedColumns.Count < 2)
                throw new ArgumentException("Для III уровня требуется минимум 2 точки в блоке.");
            if (rigidTolerance <= 0)
                throw new ArgumentOutOfRangeException(nameof(rigidTolerance), "Допуск жёсткости должен быть больше 0.");
            Level3StructuralResult result = new Level3StructuralResult
            {
                BlockName = blockName,
                MethodType = methodType
            };
            List<RigidLink> links = BuildRigidLinks(cycles, selectedColumns, rigidTolerance);
            foreach (var link in links)
                result.Links.Add(link);
            List<StructuralCluster> clusters;
            clusters = BuildStructuralClustersHierarchical(selectedColumns, links);
            foreach (var cluster in clusters)
                result.Clusters.Add(cluster);
            foreach (string point in selectedColumns)
            {
                bool existsInCluster = result.Clusters.Any(c => c.PointNames.Contains(point));
                if (!existsInCluster)
                    result.StandalonePoints.Add(point);
            }
            foreach (string point in result.StandalonePoints)
            {
                StructuralCluster isolated = new StructuralCluster
                {
                    ClusterId = result.Clusters.Count,
                    IsIsolatedPoint = true
                };
                isolated.PointNames.Add(point);
                result.Clusters.Add(isolated);
            }
            result.RecommendedClusterId = SelectMostProblematicCluster(result, links);
            return result;
        }
        private List<RigidLink> BuildRigidLinks(
    List<MeasurementCycle> cycles,
    List<string> selectedColumns,
    double rigidTolerance)
        {
            List<RigidLink> links = new List<RigidLink>();
            for (int i = 0; i < selectedColumns.Count; i++)
            {
                for (int j = i + 1; j < selectedColumns.Count; j++)
                {
                    string pointA = selectedColumns[i];
                    string pointB = selectedColumns[j];
                    double referenceDistance = Math.Abs(
                        cycles[0].PointValues[pointA] - cycles[0].PointValues[pointB]);
                    double maxDeviation = 0.0;
                    foreach (MeasurementCycle cycle in cycles)
                    {
                        double currentDistance = Math.Abs(
                            cycle.PointValues[pointA] - cycle.PointValues[pointB]);
                        double deviation = Math.Abs(currentDistance - referenceDistance);
                        if (deviation > maxDeviation)
                            maxDeviation = deviation;
                    }
                    links.Add(new RigidLink
                    {
                        PointA = pointA,
                        PointB = pointB,
                        ReferenceDistance = RoundValue(referenceDistance),
                        MaxDeviation = RoundValue(maxDeviation),
                        IsRigid = maxDeviation <= rigidTolerance
                    });
                }
            }
            return links;
        }
        private List<StructuralCluster> BuildStructuralClustersHierarchical(
    List<string> selectedColumns,
    List<RigidLink> links)
        {
            Dictionary<string, HashSet<string>> adjacency = new Dictionary<string, HashSet<string>>();
            foreach (string point in selectedColumns)
                adjacency[point] = new HashSet<string>();
            foreach (RigidLink link in links.Where(l => l.IsRigid))
            {
                adjacency[link.PointA].Add(link.PointB);
                adjacency[link.PointB].Add(link.PointA);
            }
            List<StructuralCluster> clusters = new List<StructuralCluster>();
            HashSet<string> visited = new HashSet<string>();
            int clusterId = 0;
            foreach (string point in selectedColumns)
            {
                if (visited.Contains(point))
                    continue;
                List<string> component = new List<string>();
                Queue<string> queue = new Queue<string>();
                queue.Enqueue(point);
                visited.Add(point);
                while (queue.Count > 0)
                {
                    string current = queue.Dequeue();
                    component.Add(current);
                    foreach (string neighbor in adjacency[current])
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
                if (component.Count > 1)
                {
                    StructuralCluster cluster = new StructuralCluster
                    {
                        ClusterId = clusterId,
                        IsIsolatedPoint = false
                    };
                    foreach (string p in component)
                        cluster.PointNames.Add(p);
                    clusters.Add(cluster);
                    clusterId++;
                }
            }
            return clusters;
        }
        private int SelectMostProblematicCluster(Level3StructuralResult result, List<RigidLink> links)
        {
            double bestScore = double.MinValue;
            int bestClusterId = -1;
            foreach (StructuralCluster cluster in result.Clusters.Where(c => !c.IsIsolatedPoint))
            {
                var clusterLinks = links.Where(l =>
                    cluster.PointNames.Contains(l.PointA) &&
                    cluster.PointNames.Contains(l.PointB));
                if (!clusterLinks.Any())
                    continue;
                double score = clusterLinks.Average(l => l.MaxDeviation);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestClusterId = cluster.ClusterId;
                }
            }
            if (bestClusterId == -1 && result.Clusters.Count > 0)
                bestClusterId = result.Clusters[0].ClusterId;
            return bestClusterId;
        }
        public Level3ResponseResult RunLevel3Response(
    List<MeasurementCycle> cycles,
    List<string> selectedColumns,
    string blockName,
    int clusterId,
    double epsilon,
    double alpha)
        {
            if (cycles == null || cycles.Count == 0)
                throw new ArgumentException("Нет циклов наблюдений для анализа.");
            if (selectedColumns == null || selectedColumns.Count < 2)
                throw new ArgumentException("Для расчёта отклика структурной части нужно минимум 2 точки.");
            if (epsilon <= 0)
                throw new ArgumentOutOfRangeException(nameof(epsilon), "E должно быть больше 0.");
            if (alpha <= 0 || alpha >= 1)
                throw new ArgumentOutOfRangeException(nameof(alpha), "A должно принадлежать интервалу (0;1).");
            double epsilonCheck = epsilon;
            List<double> baseVector = ExtractVector(cycles[0], selectedColumns);
            List<double> baseShapeVector = CenterVector(baseVector);
            List<double> baseShapeVectorPlus = CenterVector(ApplyAlternatingEpsilon(baseVector, epsilonCheck));
            List<double> baseShapeVectorMinus = CenterVector(ApplyAlternatingEpsilon(baseVector, -epsilonCheck));
            double baseM = CalculateNorm(baseVector);
            Level3ResponseResult result = new Level3ResponseResult
            {
                BlockName = blockName,
                ClusterId = clusterId,
                InputEpsilon = epsilon,
                CheckEpsilon = epsilonCheck,
                Alpha = alpha
            };
            foreach (MeasurementCycle cycle in cycles)
            {
                List<double> currentVector = ExtractVector(cycle, selectedColumns);
                List<double> currentVectorPlus = currentVector.Select(v => v + epsilonCheck).ToList();
                List<double> currentVectorMinus = currentVector.Select(v => v - epsilonCheck).ToList();
                List<double> currentShapeVector = CenterVector(currentVector);
                List<double> currentShapeVectorPlus = CenterVector(ApplyAlternatingEpsilon(currentVector, epsilonCheck));
                List<double> currentShapeVectorMinus = CenterVector(ApplyAlternatingEpsilon(currentVector, -epsilonCheck));
                double m = CalculateNorm(currentVector);
                double mPlus = CalculateNorm(currentVectorPlus);
                double mMinus = CalculateNorm(currentVectorMinus);
                double a = CalculateResponseAngleByDisplacement(baseVector, currentVector);
                double aPlus = CalculateResponseAngleByDisplacement(baseVector, currentVectorPlus);
                double aMinus = CalculateResponseAngleByDisplacement(baseVector, currentVectorMinus);
                double r = Math.Abs(mPlus - mMinus) / 2.0;
                double l = Math.Abs(m - baseM);
                result.Items.Add(new PhaseStateItem
                {
                    Label = cycle.Label,
                    M = RoundValue(m),
                    MPlusE = RoundValue(mPlus),
                    MMinusE = RoundValue(mMinus),
                    A = RoundValue(a),
                    APlusE = RoundValue(aPlus),
                    AMinusE = RoundValue(aMinus),
                    R = RoundValue(r),
                    L = RoundValue(l),
                    StatusText = GetStatusText(r, l)
                });
            }
            FillForecastLevel3Response(result, alpha, baseM);
            return result;
        }
        private static void FillForecastLevel3Response(Level3ResponseResult result, double alpha, double baseM)
        {
            List<double> mValues = result.Items.Select(x => x.M).ToList();
            List<double> mPlusValues = result.Items.Select(x => x.MPlusE).ToList();
            List<double> mMinusValues = result.Items.Select(x => x.MMinusE).ToList();
            List<double> aValues = result.Items.Select(x => x.A).ToList();
            List<double> aPlusValues = result.Items.Select(x => x.APlusE).ToList();
            List<double> aMinusValues = result.Items.Select(x => x.AMinusE).ToList();
            result.ForecastM = RoundValue(ExponentialForecast(mValues, alpha));
            result.ForecastMPlusE = RoundValue(ExponentialForecast(mPlusValues, alpha));
            result.ForecastMMinusE = RoundValue(ExponentialForecast(mMinusValues, alpha));
            result.ForecastA = RoundValue(ExponentialForecast(aValues, alpha));
            result.ForecastAPlusE = RoundValue(ExponentialForecast(aPlusValues, alpha));
            result.ForecastAMinusE = RoundValue(ExponentialForecast(aMinusValues, alpha));
            result.ForecastR = RoundValue(Math.Abs(result.ForecastMPlusE - result.ForecastMMinusE) / 2.0);
            result.ForecastL = RoundValue(Math.Abs(result.ForecastM - baseM));
            result.ForecastStatusText = GetStatusText(result.ForecastR, result.ForecastL);
        }
        public ClusterAnalysisResult RunLevel3(
    List<MeasurementCycle> cycles,
    List<string> selectedColumns,
    string blockName,
    ClusteringMethodType methodType,
    double dbscanEps = 0.85,
    int dbscanMinPoints = 3,
    int kMeansK = 3)
        {
            if (cycles == null || cycles.Count == 0)
                throw new ArgumentException("Нет циклов наблюдений для анализа.");
            if (selectedColumns == null || selectedColumns.Count == 0)
                throw new ArgumentException("Не выбраны точки блока для III уровня.");
            List<ClusterFeature> features = BuildClusterFeatures(cycles, selectedColumns);
            NormalizeClusterFeatures(features);
            IClusteringMethod method = CreateClusteringMethod(methodType, dbscanEps, dbscanMinPoints, kMeansK);
            return method.Cluster(blockName, features);
        }
        private IClusteringMethod CreateClusteringMethod(
            ClusteringMethodType methodType,
            double dbscanEps,
            int dbscanMinPoints,
            int kMeansK)
        {
            return new HierarchicalClusteringService();
        }
        private List<ClusterFeature> BuildClusterFeatures(
            List<MeasurementCycle> cycles,
            List<string> selectedColumns)
        {
            List<ClusterFeature> features = new List<ClusterFeature>();
            foreach (string pointName in selectedColumns)
            {
                List<double> values = new List<double>();
                foreach (MeasurementCycle cycle in cycles)
                {
                    if (!cycle.PointValues.ContainsKey(pointName))
                        throw new InvalidOperationException($"В цикле {cycle.Label} отсутствует точка {pointName}.");
                    values.Add(cycle.PointValues[pointName]);
                }
                ClusterFeature feature = new ClusterFeature
                {
                    PointName = pointName,
                    Mean = RoundValue(CalculateMean(values)),
                    Variance = RoundValue(CalculateVariance(values)),
                    Range = RoundValue(CalculateRange(values)),
                    MeanVelocity = RoundValue(CalculateMeanVelocity(values))
                };
                features.Add(feature);
            }
            return features;
        }
        private void NormalizeClusterFeatures(List<ClusterFeature> features)
        {
            if (features == null || features.Count == 0)
                return;
            double meanMean = features.Average(f => f.Mean);
            double meanVariance = features.Average(f => f.Variance);
            double meanRange = features.Average(f => f.Range);
            double meanVelocity = features.Average(f => f.MeanVelocity);
            double stdMean = CalculateStd(features.Select(f => f.Mean).ToList(), meanMean);
            double stdVariance = CalculateStd(features.Select(f => f.Variance).ToList(), meanVariance);
            double stdRange = CalculateStd(features.Select(f => f.Range).ToList(), meanRange);
            double stdVelocity = CalculateStd(features.Select(f => f.MeanVelocity).ToList(), meanVelocity);
            foreach (var feature in features)
            {
                feature.NormalizedMean = NormalizeValue(feature.Mean, meanMean, stdMean);
                feature.NormalizedVariance = NormalizeValue(feature.Variance, meanVariance, stdVariance);
                feature.NormalizedRange = NormalizeValue(feature.Range, meanRange, stdRange);
                feature.NormalizedMeanVelocity = NormalizeValue(feature.MeanVelocity, meanVelocity, stdVelocity);
            }
        }
        #endregion

        #region 8. Математический аппарат IV уровня декомпозиции

        public Level4AnalysisResult RunLevel4(
            List<MeasurementCycle> cycles,
            string pointName,
            double epsilon,
            double alpha)
        {
            if (string.IsNullOrWhiteSpace(pointName))
                throw new ArgumentException("Не выбрана контрольная точка.");
            return RunLevel4(cycles, new List<string> { pointName }, epsilon, alpha);
        }
        public Level4AnalysisResult RunLevel4(
            List<MeasurementCycle> cycles,
            List<string> pointNames,
            double epsilon,
            double alpha)
        {
            if (cycles == null || cycles.Count == 0)
                throw new ArgumentException("Нет циклов наблюдений для анализа IV уровня.");
            if (pointNames == null || pointNames.Count == 0)
                throw new ArgumentException("Не выбраны контрольные точки для IV уровня.");
            if (epsilon <= 0)
                throw new ArgumentOutOfRangeException(nameof(epsilon), "E должно быть больше 0.");
            if (alpha <= 0 || alpha >= 1)
                throw new ArgumentOutOfRangeException(nameof(alpha), "A должно принадлежать интервалу (0;1).");
            List<string> selectedPoints = pointNames
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct()
                .ToList();
            if (selectedPoints.Count == 0)
                throw new ArgumentException("Не выбраны контрольные точки для IV уровня.");
            foreach (string pointName in selectedPoints)
            {
                if (!cycles[0].PointValues.ContainsKey(pointName))
                    throw new InvalidOperationException($"Точка {pointName} отсутствует в исходных данных.");
            }
            Level4AnalysisResult result = new Level4AnalysisResult
            {
                PointName = string.Join(", ", selectedPoints)
            };
            foreach (string pointName in selectedPoints)
                result.PointNames.Add(pointName);
            foreach (string pointName in selectedPoints)
            {
                double baseZ = cycles[0].PointValues[pointName];
                for (int i = 0; i < cycles.Count; i++)
                {
                    MeasurementCycle cycle = cycles[i];
                    if (!cycle.PointValues.ContainsKey(pointName))
                        throw new InvalidOperationException($"В цикле {cycle.Label} отсутствует точка {pointName}.");
                    double z = cycle.PointValues[pointName];
                    double zPlus = z + epsilon;
                    double zMinus = z - epsilon;
                    double r = epsilon;
                    double l = Math.Abs(z - baseZ);
                    result.Items.Add(new Level4PointStateItem
                    {
                        CycleLabel = cycle.Label,
                        PointName = pointName,
                        Z = RoundValue(z),
                        ZPlusE = RoundValue(zPlus),
                        ZMinusE = RoundValue(zMinus),
                        R = RoundValue(r),
                        L = RoundValue(l),
                        StatusText = GetStatusText(r, l)
                    });
                }
                List<double> zValues = result.Items
                    .Where(item => item.PointName == pointName)
                    .Select(item => item.Z)
                    .ToList();
                double forecastZ = ExponentialForecast(zValues, alpha);
                double forecastL = Math.Abs(forecastZ - baseZ);
                Level4PointForecastItem forecast = new Level4PointForecastItem
                {
                    PointName = pointName,
                    ForecastZ = RoundValue(forecastZ),
                    ForecastZPlusE = RoundValue(forecastZ + epsilon),
                    ForecastZMinusE = RoundValue(forecastZ - epsilon),
                    R = RoundValue(epsilon),
                    L = RoundValue(forecastL),
                    ForecastStatusText = GetStatusText(epsilon, forecastL)
                };
                result.Forecasts.Add(forecast);
            }
            if (result.Forecasts.Count > 0)
            {
                Level4PointForecastItem firstForecast = result.Forecasts[0];
                double firstBaseZ = cycles[0].PointValues[firstForecast.PointName];
                result.ForecastZ = firstForecast.ForecastZ;
                result.ForecastZPlusE = firstForecast.ForecastZPlusE;
                result.ForecastZMinusE = firstForecast.ForecastZMinusE;
                result.ForecastDelta = RoundValue(firstForecast.ForecastZ - firstBaseZ);
                result.ForecastVelocity = 0.0;
                result.ForecastAcceleration = 0.0;
                result.ForecastStatusText = firstForecast.ForecastStatusText;
            }
            return result;
        }
        #endregion

        #region 9. Общие математические функции

        private static List<double> ExtractVector(MeasurementCycle cycle, List<string> pointNames)
        {
            List<double> vector = new List<double>();
            foreach (string pointName in pointNames)
            {
                if (!cycle.PointValues.ContainsKey(pointName))
                    throw new InvalidOperationException($"В цикле {cycle.Label} отсутствует точка {pointName}.");
                vector.Add(cycle.PointValues[pointName]);
            }
            return vector;
        }
        private static double CalculateNorm(List<double> vector)
        {
            double sum = 0.0;
            foreach (double value in vector)
            {
                sum += value * value;
            }
            return Math.Sqrt(sum);
        }
        private static List<double> CenterVector(List<double> vector)
        {
            if (vector == null || vector.Count == 0)
                return new List<double>();
            double mean = vector.Average();
            return vector.Select(v => v - mean).ToList();
        }
        private static List<double> ApplyAlternatingEpsilon(List<double> vector, double epsilon)
        {
            if (vector == null)
                return new List<double>();
            List<double> result = new List<double>(vector.Count);
            for (int i = 0; i < vector.Count; i++)
            {
                double sign = (i % 2 == 0) ? 1.0 : -1.0;
                result.Add(vector[i] + sign * epsilon);
            }
            return result;
        }
        private static double CalculateResponseAngleByDisplacement(List<double> baseVector, List<double> currentVector)
        {
            if (baseVector == null || currentVector == null || baseVector.Count == 0 || currentVector.Count == 0)
                return 0.0;
            int count = Math.Min(baseVector.Count, currentVector.Count);
            double displacementSquared = 0.0;
            double baseSquared = 0.0;
            for (int i = 0; i < count; i++)
            {
                double d = currentVector[i] - baseVector[i];
                displacementSquared += d * d;
                baseSquared += baseVector[i] * baseVector[i];
            }
            double displacementNorm = Math.Sqrt(displacementSquared);
            double baseNorm = Math.Sqrt(baseSquared);
            if (baseNorm < 1e-12)
                baseNorm = 1e-12;
            return Math.Atan2(displacementNorm, baseNorm);
        }
        

        private static double CalculateAngle(List<double> baseVector, List<double> currentVector)
        {
            double normBase = CalculateNorm(baseVector);
            double normCurrent = CalculateNorm(currentVector);
            if (normBase == 0 || normCurrent == 0)
                return 0.0;
            double dot = 0.0;
            for (int i = 0; i < baseVector.Count; i++)
                dot += baseVector[i] * currentVector[i];
            double projectionCoefficient = dot / (normBase * normBase);
            double orthogonalNormSquared = 0.0;
            for (int i = 0; i < baseVector.Count; i++)
            {
                double orthogonalComponent = currentVector[i] - projectionCoefficient * baseVector[i];
                orthogonalNormSquared += orthogonalComponent * orthogonalComponent;
            }
            double orthogonalNorm = Math.Sqrt(Math.Max(0.0, orthogonalNormSquared));
            return Math.Atan2(normBase * orthogonalNorm, dot);
        }
        private static double ExponentialForecast(List<double> values, double alpha)
        {
            if (values == null || values.Count == 0)
                return 0.0;
            double smoothed = values[0];
            for (int i = 1; i < values.Count; i++)
            {
                smoothed = alpha * values[i] + (1.0 - alpha) * smoothed;
            }
            return smoothed;
        }
        private static string GetStatusText(double r, double l)
        {
            double tolerance = Math.Max(1e-9, r * 0.01);
            if (l < r - tolerance)
                return "Без изменений";
            if (Math.Abs(l - r) <= tolerance)
                return "Предаварийное";
            return "Аварийное";
        }
        private static double CalculateStd(List<double> values, double mean)
        {
            if (values == null || values.Count == 0)
                return 1.0;
            double variance = values.Select(v => (v - mean) * (v - mean)).Average();
            double std = Math.Sqrt(variance);
            return std < 1e-12 ? 1.0 : std;
        }
        private static double NormalizeValue(double value, double mean, double std)
        {
            return (value - mean) / std;
        }
        private static double CalculateMean(List<double> values)
        {
            if (values == null || values.Count == 0)
                return 0.0;
            return values.Average();
        }
        private static double CalculateVariance(List<double> values)
        {
            if (values == null || values.Count == 0)
                return 0.0;
            double mean = values.Average();
            return values.Select(v => (v - mean) * (v - mean)).Average();
        }
        private static double CalculateRange(List<double> values)
        {
            if (values == null || values.Count == 0)
                return 0.0;
            return values.Max() - values.Min();
        }
        private static double CalculateMeanVelocity(List<double> values)
        {
            if (values == null || values.Count < 2)
                return 0.0;
            List<double> deltas = new List<double>();
            for (int i = 1; i < values.Count; i++)
                deltas.Add(values[i] - values[i - 1]);
            return deltas.Average();
        }
        private static double RoundValue(double value)
        {
            return Math.Round(value, 7);
        }

        #endregion
    }
}
