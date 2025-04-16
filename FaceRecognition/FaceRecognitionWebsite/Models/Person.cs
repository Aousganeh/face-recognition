namespace FaceRecognitionWebsite.Models;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<FaceImage> Images { get; set; }
}