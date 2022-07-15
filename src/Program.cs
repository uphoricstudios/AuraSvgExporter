using System.CommandLine;
using System.Drawing;
using System.Drawing.Imaging;
using Svg;

const string OutputFolderName = "AuraSvgOutput";

var outputFolderPath = String.Empty;
var inputFile = new Argument<FileSystemInfo>("input").ExistingOnly();
var outputFile = new Option<DirectoryInfo>("--output").ExistingOnly();
var sizes = new Option<int[]>("--sizes");
var command = new RootCommand();

inputFile.Arity = ArgumentArity.ExactlyOne;
inputFile.Description = @"The SVG file(s) to export into a different image format. 
If directory is specified, the entire directory will be processed.";

sizes.AddAlias("-s");
sizes.Description = "The different sizes to export the SVG to.";
sizes.IsRequired = true;
sizes.AllowMultipleArgumentsPerToken = true;

outputFile.AddAlias("-o");
outputFile.Description = "Output directory.";
outputFile.IsRequired = false;

command.AddArgument(inputFile);
command.AddOption(sizes);
command.Add(outputFile);
command.Description = @"A tool that exports SVGs into different image formats with different sizes. 
It currently only exports to png format, more will be added if there is a demand.";


#region Methods 

void processDirectory(DirectoryInfo dirInfo, int[] sizes) {
    List<String> svgFiles = Directory.EnumerateFiles(dirInfo.FullName, "*.svg").ToList();
    int count = 0;
    
    svgFiles.ForEach(file => {
            Console.WriteLine($"({++count}/{svgFiles.Count}) Processing: {Path.GetFileName(file)}");
            svg2Png(file, sizes);
    });
}

void processFile(FileInfo fileInfo, int[] sizes) {
    Console.WriteLine($"Processing: {fileInfo.Name}");
    svg2Png(fileInfo.FullName, sizes);
}

void createOutputFolder(DirectoryInfo outputFileInfo) {
    if (outputFileInfo != null) {
        outputFolderPath = outputFileInfo.FullName;
        return;
    }
   
   outputFolderPath = Path.Combine(Directory.GetCurrentDirectory(), OutputFolderName);

    try {
        if(!Directory.Exists(outputFolderPath)) {
            Console.WriteLine("Creating output directory...");
            DirectoryInfo dirInfo = Directory.CreateDirectory(outputFolderPath);
            Console.WriteLine($"{dirInfo.FullName} created.");
        }
    }

    catch(Exception e) {
        Console.WriteLine("The process failed: {0}", e.ToString());
    }
}

void svg2Png(string svgPath, int[] sizes) {
    String fileName = Path.GetFileNameWithoutExtension(svgPath);
    String outputSubFolder = Path.Combine(outputFolderPath, fileName);
    
    if(!Directory.Exists(outputSubFolder)) {
        DirectoryInfo dirInfo = Directory.CreateDirectory(outputSubFolder);
    }
    
    foreach(int size in sizes) {
        string exportedFileName = Path.Combine(outputSubFolder, $"{fileName}-{size}.png");
        
        SvgDocument svgDocument = SvgDocument.Open(svgPath);
        svgDocument.ShapeRendering = SvgShapeRendering.Auto;
        
        Bitmap bmp = svgDocument.Draw(size,size);
        bmp.Save(exportedFileName, ImageFormat.Png);
    }
}

#endregion

command.SetHandler<FileSystemInfo, int[], DirectoryInfo>(
    (inputFile, sizes, outputFile) => {
        createOutputFolder(outputFile);
        Console.WriteLine($"Output Directory: {outputFolderPath}");

        foreach(int size in sizes) {
            if(size < 1) {
                Console.WriteLine("The size must be greater than 0.");
                return;
            }
        }

        if(inputFile is FileInfo) {
            processFile((inputFile as FileInfo)!, sizes);
        }

        else if(inputFile is DirectoryInfo) {
            processDirectory((inputFile as DirectoryInfo)!, sizes);
        }

    }, inputFile, sizes, outputFile
);

command.Invoke(args);