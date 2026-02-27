namespace LightNovelCore.Models.Types;

/// <summary>
/// The publication format of a novel
/// </summary>
public enum LncFormat
{
	/// <summary>
	/// A physical release (e.g. paperback, hardcover, etc.)
	/// </summary>
	[Display(Name = "Physical")]
	[Description("A physical release (e.g. paperback, hardcover, etc.)")]
	Physical = 0,
	/// <summary>
	/// A digital release (e.g. EPub, PDF, etc.)
	/// </summary>
	[Display(Name = "Digital")]
	[Description("A digital release (e.g. EPub, PDF, etc.)")]
	Digital = 1,
	/// <summary>
	/// An audio release (e.g. Audible, etc.)
	/// </summary>
	[Display(Name = "Audio")]
	[Description("An audio release (e.g. Audible, etc.)")]
	Audio = 2,
}
