using ConfiguratorWebApp.Data;
using ConfiguratorWebApp.Models.Entities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ConfiguratorWebApp.Services;

public class ConfigurationService(ApplicationDbContext dbContext)
{
	private readonly ApplicationDbContext _dbContext = dbContext;
	private readonly string _logFilePath = "./Data/log.txt";

	// Method of determining the file type and using the correct function
	public void LoadConfiguration(string filePath)
	{
		try
		{
			var extension = Path.GetExtension(filePath);
			if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
			{
				LoadConfigurationFromJson(filePath);
			}
			else if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
			{
				LoadConfigurationFromTxt(filePath);
			}
			else
			{
				throw new ArgumentException("Unsupported file extension");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error loading configuration: {ex.Message}");
			throw;
		}
	}

	// Method to get configurations from the database
	public List<Configuration> GetConfiguration(string path)
	{
		try
		{
			// If the path is empty or null, we return the entire configuration
			if (string.IsNullOrEmpty(path))
			{
				return [.. _dbContext.Configurations];
			}

			// Find the new root element
			Guid? parentId = null;
			var segments = path.Split('/');
			foreach (var segment in segments)
			{
				parentId = FindConfigurationPartRoot(segment, parentId);

				// Check if a branch was found for the current path
				if (parentId == null)
				{
					throw new ArgumentException($"Branch '{segment}' in '{path}' not found in the configuration tree.");
				}
			}

			// Find part of tree
			List<Configuration> configurations = [];

			LogLine("Configuration tree part elements:");
			FindConfigurationPartRecursive(configurations, parentId);

			return configurations;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error getting configuration: {ex.Message}");
			throw;
		}
	}

	/* ------------------------- GetConfiduraton methods ------------------------- */
	// Find element root id by path value
	private Guid? FindConfigurationPartRoot(string segment, Guid? parentId)
	{
		var parent = _dbContext.Configurations
			.FirstOrDefault(c => c.Key == segment && c.ParentId == parentId);

		return parent?.Id;
	}

	// Find part of tree by path value
	private void FindConfigurationPartRecursive(List<Configuration> configurations, Guid? parentId, bool isFirstCall = true)
	{
		var root = _dbContext.Configurations
		.Where(c => c.ParentId == parentId)
		.ToList();

		// Refer to the leaf element
		if (isFirstCall && root.Count == 0)
		{
			var element = _dbContext.Configurations.FirstOrDefault(conf => conf.Id == parentId);
			if (element != null)
			{
				LogLine($"Key: {element.Key}, Value: {element.Value}");
				
				element.ParentId = null;
				element.Key = "";
				configurations.Add(element);
			}

			return;
		}

		// Refer to the branch element
		foreach (var conf in root)
		{
			LogLine($"Key: {conf.Key}, ParentId: {conf.ParentId}, Value: {conf.Value}");
			
			if (isFirstCall)
			{
				conf.ParentId = null;
			}

			configurations.Add(conf);

			// If it`s a branch element
			if (conf.Value == null)
			{
				FindConfigurationPartRecursive(configurations, conf.Id, false);
			}
		}
	}

	/* ------------------------- LoadConfiguration methods ------------------------- */
	// Method to load configuration from JSON file
	private void LoadConfigurationFromJson(string filePath)
	{
		try
		{
			LogLine($"Start parsing {filePath}", clean: true);

			var json = File.ReadAllText(filePath);
			if (json.Length == 0)
			{
				throw new Exception("Error format: empty JSON configuration file");
			}

			LogLine("Configuration data:");
			var configuration = ParseJsonRecursive(json, null);

			SaveConfiguration(configuration);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error loading JSON configuration: {ex.Message}");
			throw;
		}
	}

	// Method to load configuration from TXT file
	private void LoadConfigurationFromTxt(string filePath)
	{
		try
		{
			LogLine($"Start parsing {filePath}", clean: true);

			var lines = File.ReadAllLines(filePath);
			if (lines.Length == 0)
			{
				throw new Exception("Error format: empty TXT configuration file");
			}

			LogLine("Configuration data:");
			var configuration = ParseTxt(lines);

			SaveConfiguration(configuration);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error loading TXT configuration: {ex.Message}");
			throw;
		}
	}

	// Parse JSON and build configuration objects
	private List<Configuration> ParseJsonRecursive(string json, Guid? parentId)
	{
		var Configurations = new List<Configuration>();
		dynamic jsonObject = JsonConvert.DeserializeObject(json);

		foreach (var property in jsonObject)
		{
			if (property.Name == "")
			{
				throw new Exception("Empty branch name");
			}

			string value = property.Value is Newtonsoft.Json.Linq.JObject ? null : property.Value.ToString();
			if (value == "")
			{
				throw new Exception("Empty leaf value");
			}

			var item = new Configuration
			{
				Id = Guid.NewGuid(),
				ParentId = parentId,
				Key = property.Name,
				Value = value
			};

			Configurations.Add(item); // Add configuration item

			LogConfiguration(item); // Log the configuration item

			if (property.Value is Newtonsoft.Json.Linq.JObject)
			{
				Configurations.AddRange(ParseJsonRecursive(property.Value.ToString(), item.Id));
			}
		}

		return Configurations;
	}

	// Parse configuration from TXT file
	private List<Configuration> ParseTxt(string[] lines)
	{
		var configurations = new List<Configuration>();

		foreach (var line in lines)
		{
			var parts = line.Split(':');
			if (parts.Length < 2)
			{
				throw new Exception($"Error format: not enough parameters in line: {line}");
			}
			else
			{
				ParseTxtLineRecursive(configurations, parts, null);
			}
		}

		return configurations;
	}

	// Parse configuration line from TXT file
	private void ParseTxtLineRecursive(List<Configuration> configurations, string[] parts, Guid? parentId)
	{
		if (string.IsNullOrEmpty(parts[0]))
		{
			throw new Exception($"Error format: empty branch value");
		}

		if (parts.Length == 2)
		{
			if (string.IsNullOrEmpty(parts[1]))
			{
				throw new Exception($"Error format: empty {parts[0]} leaf value");
			}

			var item = new Configuration
			{
				Id = Guid.NewGuid(),
				ParentId = parentId,
				Key = parts[0],
				Value = parts[1]
			};

			configurations.Add(item);
			LogConfiguration(item);
		}
		else
		{
			// Checking if this branch already exists
			var id = FindConfiguration(configurations, parentId, parts[0]);
			if (id != null)
			{
				parentId = id;
			}
			else
			{
				var item = new Configuration
				{
					Id = Guid.NewGuid(),
					ParentId = parentId,
					Key = parts[0],
					Value = null
				};

				configurations.Add(item);
				LogConfiguration(item);

				parentId = item.Id;
			}

			ParseTxtLineRecursive(configurations, parts.Skip(1).ToArray(), parentId);
		}
	}

	// Find element Guid
	private static Guid? FindConfiguration(List<Configuration> configurations, Guid? parentId, string key)
	{
		var element = configurations.FirstOrDefault(conf => conf.ParentId == parentId && conf.Key == key);

		return element?.Id;
	}

	// Method to save configurations to the database
	private void SaveConfiguration(List<Configuration> configuration)
	{
		// Remove existing configurations
		_dbContext.Configurations.RemoveRange(_dbContext.Configurations);

		// Add new configurations
		_dbContext.Configurations.AddRange(configuration);
		_dbContext.SaveChanges();

		LogLine("Configuration saved successefull");
	}

	/* ------------------------- Logger methods ------------------------- */
	// Log information about a config item or a simple line to a log file
	private void LogConfiguration(Configuration item)
	{
		using StreamWriter writer = new(_logFilePath, true);
		writer.WriteLine($"Id: {item.Id}, ParentId: {item.ParentId}, Key: {item.Key}, Value: {item.Value}");
	}

	private void LogLine(string line = "", bool clean = false)
	{
		using StreamWriter writer = new(_logFilePath, !clean);
		writer.WriteLine(line);
	}

	/* -------------------------- Load data to file methods -------------------------- */
	public void LoadConfigurationToFile(string fileName)
	{
		try
		{
			var directory = Path.GetDirectoryName(fileName);
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			var allConfigurations = _dbContext.Configurations.ToList();

			using StreamWriter writer = new(fileName);
			writer.WriteLine("{");

			writer.WriteLine(LoadConfigurationToFileRecursive(null, 1, allConfigurations));

			writer.WriteLine("}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error saving configuration: {ex.Message}");
			throw;
		}
	}

	private string LoadConfigurationToFileRecursive(Guid? parentId, int level, List<Configuration> allConfigurations)
	{
		var indentation = new string('\t', level);
		string elementBody = "";

		var root = _dbContext.Configurations
		.Where(c => c.ParentId == parentId)
		.ToList();

		foreach (var conf in root)
		{
			var isLeaf = !allConfigurations.Any(c => c.ParentId == conf.Id);

			if (isLeaf)
			{
				elementBody += $"{indentation}\"{conf.Key}\": \"{conf.Value}\",\n";
			}
			else
			{
				elementBody += $"{indentation}\"{conf.Key}\": {{\n";

				elementBody += LoadConfigurationToFileRecursive(conf.Id, level + 1, allConfigurations);
				elementBody = elementBody.TrimEnd(',', ' ', '\n') + $"\n{indentation}}},\n";
			}
		}

		if(level == 1) {
			elementBody = elementBody[..^2];
		}
		return elementBody;
	}
}
