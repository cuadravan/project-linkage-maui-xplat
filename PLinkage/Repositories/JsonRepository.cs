using Newtonsoft.Json;
using PLinkage.Interfaces;
using System.Reflection;

namespace PLinkage.Repositories
{
    public class JsonRepository<T> : IRepository<T> where T : class
    {
        private readonly string _filePath;
        private List<T> _data;

        public JsonRepository(string fileName)
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory;
            string jsonFolderPath = Path.GetFullPath(Path.Combine(projectPath, @"..\..\..\..\..\json"));

            Directory.CreateDirectory(jsonFolderPath);

            _filePath = Path.Combine(jsonFolderPath, $"{fileName}.txt");

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }

            try
            {
                var json = File.ReadAllText(_filePath);
                _data = JsonConvert.DeserializeObject<List<T>>(json) ?? new();
            }
            catch
            {
                _data = new();
            }
        }

        public Task<List<T>> GetAllAsync() => Task.FromResult(_data);

        public Task<T?> GetByIdAsync(Guid id)
        {
            var prop = GetIdProperty();
            if (prop == null) return Task.FromResult<T?>(null);

            var match = _data.FirstOrDefault(item =>
                prop.GetValue(item) is Guid value && value == id);

            return Task.FromResult(match);
        }

        public Task AddAsync(T entity)
        {
            _data.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(T entity)
        {
            var prop = GetIdProperty();
            if (prop == null) return Task.CompletedTask;

            var id = prop.GetValue(entity) as Guid?;
            if (id == null) return Task.CompletedTask;

            var index = _data.FindIndex(item =>
                prop.GetValue(item) is Guid value && value == id);

            if (index >= 0)
            {
                _data[index] = entity;
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            var prop = GetIdProperty();
            if (prop == null) return Task.CompletedTask;

            var entity = _data.FirstOrDefault(item =>
                prop.GetValue(item) is Guid value && value == id);

            if (entity != null)
            {
                _data.Remove(entity);
            }

            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            var json = JsonConvert.SerializeObject(_data, Formatting.Indented);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public Task Reload()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _data = JsonConvert.DeserializeObject<List<T>>(json) ?? new();
            }

            return Task.CompletedTask;
        }

        private PropertyInfo? GetIdProperty()
        {
            return typeof(T).GetProperties()
                .FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
                                  && p.PropertyType == typeof(Guid));
        }
    }
}
