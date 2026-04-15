/**
 * Compresses an image file if it exceeds the specified size limit.
 * 
 * @param file - The image file to compress.
 * @param maxSizeMB - The maximum allowed file size in Megabytes. Default is 5MB.
 * @returns A Promise that resolves to the compressed File or the original File if no compression was needed.
 */
export const compressImage = async (file: File, maxSizeMB: number = 5): Promise<File> => {
  const MAX_SIZE_BYTES = maxSizeMB * 1024 * 1024;

  // If file is smaller than the limit or not an image, return original
  if (file.size <= MAX_SIZE_BYTES || !file.type.startsWith('image/')) {
    return file;
  }

  return new Promise((resolve, reject) => {
    const img = new Image();
    const url = URL.createObjectURL(file);

    img.onload = () => {
      URL.revokeObjectURL(url);
      
      let quality = 0.7; // Start with 70% quality
      const canvas = document.createElement('canvas');
      let width = img.width;
      let height = img.height;

      // Initial dimensions
      canvas.width = width;
      canvas.height = height;
      
      const ctx = canvas.getContext('2d');
      if (!ctx) {
        // Fallback if canvas context fails
        resolve(file);
        return;
      }

      ctx.drawImage(img, 0, 0, width, height);

      const compress = () => {
        canvas.toBlob(
          (blob) => {
            if (!blob) {
              resolve(file);
              return;
            }

            if (blob.size <= MAX_SIZE_BYTES || quality <= 0.1) {
              // Convert blob back to File
              const compressedFile = new File([blob], file.name, {
                type: file.type,
                lastModified: Date.now(),
              });
              resolve(compressedFile);
            } else {
              // Reduce quality and try again
              quality -= 0.1;
              // Also scale down if quality gets too low to preserve some visual integrity
              if (quality < 0.5) {
                width *= 0.9;
                height *= 0.9;
                canvas.width = width;
                canvas.height = height;
                ctx.clearRect(0, 0, width, height);
                ctx.drawImage(img, 0, 0, width, height);
              }
              compress();
            }
          },
          file.type,
          quality
        );
      };

      compress();
    };

    img.onerror = (error) => {
      URL.revokeObjectURL(url);
      reject(error);
    };

    img.src = url;
  });
};
