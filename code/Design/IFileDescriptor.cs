namespace ManagedX.Content.Design
{

	/// <summary>Base interface for file descriptors.</summary>
	public interface IFileDescriptor
	{

		/// <summary>Gets name of the file; must not be null.</summary>
		string Name { get; }


		/// <summary>Gets the position of the file in the archive stream; must not be negative.</summary>
		long Position { get; }


		/// <summary>Gets the length, in bytes, of the file in the archive; must not be negative.
		/// <para>If the file is compressed, this is the compressed size.</para>
		/// </summary>
		long Length { get; }

	}

}