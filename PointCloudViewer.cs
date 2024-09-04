using System;
using System.Collections.Generic;
using System.IO;
using NXOpen;

namespace WednesdayTaskNX
{
    public class PointCloudViewer
    {
        public static void Main()
        {
            try
            {
                // Get the NX session
                Session session = Session.GetSession();
                if (session == null)
                {
                    Console.WriteLine("Failed to get NX session.");
                    return;
                }

                // Get the active part
                Part workPart = session.Parts.Work;
                if (workPart == null)
                {
                    session.ListingWindow.WriteLine("No active part found. Please open a part.");
                    return;
                }

                // Load the point cloud data
                string filePath = @"C:\Users\diksh\open3d_data\download\BunnyMesh\BunnyMesh.ply";
                List<Point3d> points = LoadPointCloud(filePath);

                // Check if points are loaded
                if (points.Count == 0)
                {
                    session.ListingWindow.WriteLine("No points loaded from file.");
                    return;
                }

                session.ListingWindow.WriteLine($"Loaded {points.Count} points from file.");

                // Visualize the point cloud
                VisualizePointCloud(workPart, points);

                // Keep NX Open running
                session.ListingWindow.Open();
                session.ListingWindow.WriteLine("Point cloud visualization complete. Press any key to exit.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static List<Point3d> LoadPointCloud(string filePath)
        {
            List<Point3d> points = new List<Point3d>();
            bool headerParsed = false;
            int vertexCount = 0;

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("element vertex"))
                        {
                            // Extract vertex count from header
                            var parts = line.Split(' ');
                            if (parts.Length == 3)
                            {
                                vertexCount = int.Parse(parts[2]);
                            }
                        }
                        else if (line.StartsWith("end_header"))
                        {
                            headerParsed = true;
                            continue;
                        }

                        if (headerParsed)
                        {
                            // Parse point data
                            var parts = line.Split(' ');
                            if (parts.Length >= 3 && vertexCount > 0)
                            {
                                double x = double.Parse(parts[0]);
                                double y = double.Parse(parts[1]);
                                double z = double.Parse(parts[2]);
                                points.Add(new Point3d(x, y, z));
                                vertexCount--;
                                if (vertexCount <= 0) break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading point cloud file: " + ex.Message);
            }

            return points;
        }

        static void VisualizePointCloud(Part workPart, List<Point3d> points)
        {
            foreach (var point in points)
            {
                try
                {
                    // Create a point in the NX model
                    Point nxPoint = workPart.Points.CreatePoint(point);
                    nxPoint.SetVisibility(SmartObject.VisibilityOption.Visible);
                }
                catch (Exception ex)
                {
                    // Log errors creating points
                    Session.GetSession().ListingWindow.WriteLine("Error creating point: " + ex.Message);
                }
            }
        }
    }
}
