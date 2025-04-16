using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using FaceRecognitionWebsite.Data;
using FaceRecognitionWebsite.Models;

public class PersonController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public PersonController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var people = _context.People.Include(p => p.Images).ToList();
        return View(people);
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile imageFile)
    {
        if (imageFile != null && imageFile.Length > 0)
        {
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadPath);

            var fileName = Path.GetRandomFileName() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            string result = RunFaceRecognitionScript(filePath);
            Console.WriteLine("📸 Python Output: " + result);

            if (!string.IsNullOrEmpty(result) && result != "UNKNOWN")
            {
                var person = _context.People.Include(p => p.Images).FirstOrDefault(p => p.Name.ToLower() == result.ToLower());
                if (person != null)
                {
                    var imagePath = "/uploads/" + fileName;
                    var newImage = new FaceImage { PersonId = person.Id, ImagePath = imagePath };
                    _context.FaceImages.Add(newImage);
                    _context.SaveChanges();

                    CopyImageToKnownFaces(person.Name, fileName);

                    ViewBag.Message = $"Matched person found: {person.Name}. Image added to their profile.";
                    return RedirectToAction("Index");
                }
            }

            ViewBag.FilePath = "/uploads/" + fileName;
            ViewBag.Message = "No match found. Please enter a name for this new person.";
            ViewBag.PythonResult = result;
            return View("NewPerson", new Person { Images = new List<FaceImage> { new FaceImage { ImagePath = "/uploads/" + fileName } } });
        }

        ViewBag.Message = "No image selected.";
        return View();
    }

    [HttpPost]
    public IActionResult AddNewPerson(Person person)
    {
        var newImage = person.Images?.FirstOrDefault();

        var existing = _context.People.Include(p => p.Images)
            .FirstOrDefault(p => p.Name.ToLower() == person.Name.ToLower());

        if (existing != null)
        {
            if (newImage != null)
            {
                newImage.PersonId = existing.Id;
                _context.FaceImages.Add(newImage);
                CopyImageToKnownFaces(existing.Name, Path.GetFileName(newImage.ImagePath));
            }
        }
        else
        {
            _context.People.Add(person);
            _context.SaveChanges();

            if (newImage != null)
            {
                newImage.PersonId = person.Id;
                _context.FaceImages.Add(newImage);
                CopyImageToKnownFaces(person.Name, Path.GetFileName(newImage.ImagePath));
            }
        }

        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    private void CopyImageToKnownFaces(string personName, string fileName)
    {
        try
        {
            var sourcePath = Path.Combine(_env.WebRootPath, "uploads", fileName);
            var extension = Path.GetExtension(fileName);
            var knownPath = Path.Combine(_env.WebRootPath, "known_faces", personName + "_" + Guid.NewGuid() + extension);

            System.IO.File.Copy(sourcePath, knownPath, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❗ Failed to copy to known_faces: {ex.Message}");
        }
    }

    [HttpGet]
    public IActionResult Photos(int id)
    {
        var person = _context.People.Include(p => p.Images).FirstOrDefault(p => p.Id == id);
        if (person == null)
        {
            return NotFound();
        }

        return View(person);
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var person = _context.People.Include(p => p.Images).FirstOrDefault(p => p.Id == id);
        if (person != null)
        {
            _context.FaceImages.RemoveRange(person.Images);
            _context.People.Remove(person);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    private string RunFaceRecognitionScript(string uploadedImagePath)
    {
        var start = new ProcessStartInfo
        {
            FileName = "/home/aousganeh/Desktop/FaceRecognition/FaceRecognitionWebsite/faceenv/bin/python",
            Arguments = $"recognize.py \"{uploadedImagePath}\" ./wwwroot/known_faces",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = start;
        process.Start();

        string output = process.StandardOutput.ReadToEnd().Trim();
        string error = process.StandardError.ReadToEnd().Trim();
        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(error))
        {
            Console.WriteLine("❗ Python Error: " + error);
            output += "\nERROR: " + error;
        }

        return output;
    }

    private void RefreshKnownFacesFromDb()
    {
        var knownFolder = Path.Combine(_env.WebRootPath, "known_faces");
        Directory.CreateDirectory(knownFolder);

        foreach (var image in _context.FaceImages.Include(i => i.Person))
        {
            var sourcePath = Path.Combine(_env.WebRootPath, image.ImagePath.TrimStart('/'));
            var destPath = Path.Combine(knownFolder, image.Person.Name + "_" + image.Id + Path.GetExtension(sourcePath));

            try
            {
                System.IO.File.Copy(sourcePath, destPath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to copy image for {image.Person.Name}: {ex.Message}");
            }
        }
    }
}
