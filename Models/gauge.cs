using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public class Gauge {

	// public string Id => $"{Owner}_{Name}";

	[Key]
	public Guid Id {get; set;}

	public int PlayerCharacterId {get; set;}

	private Dictionary<string, string> _colourLookup = new Dictionary<string, string>
	{
		{ "hp", "red" },
		{ "health", "red" },
		{ "energy", "blue" },
		{ "armour", "yellow" },
		{ "armor", "yellow" },
		{ "soul", "purple" }
	};


	public string? Icon {get; set;}

	public string Name {get; set;}
	public int Value {get; set;}
	public int Max {get; set;}


	public string Colour => _colourLookup.TryGetValue(Name?.ToLower(), out var colour) ? colour : null;	
}
