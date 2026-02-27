namespace LightNovelCore.DataSet;

/// <summary>
/// The publication format of a novel
/// </summary>
[Flags]
public enum PublicationFormat : byte
{
	/// <summary>
	/// Has a physical release (e.g. paperback, hardcover, etc.)
	/// </summary>
	Physical = 1,
	/// <summary>
	/// Has a digital EPub or PDF release
	/// </summary>
	Digital = 2,
	/// <summary>
	/// Has an audio book release (e.g. Audible, etc.)
	/// </summary>
	Audio = 4,
}
