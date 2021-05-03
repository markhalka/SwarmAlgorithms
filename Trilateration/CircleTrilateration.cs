using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*MIT License

Copyright (c) 2020 Ahmed Elkalaf

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

// code borrowed from https://github.com/Kallaf/Trilateration

// CircleTrilateration
// this class is responsible for finding the position of unknown robots using trilateration

public class CircleTrilateration
{
    // intersectionOf2Cirlces(center0, radius0, center1, radius1, out intersection1, out intersection2) returns the intersections of provided circles
    public static int intersectionOf2Circles(Vector3 center0, float radius0, Vector3 center1, float radius1, out Vector3 intersection1, out Vector3 intersection2)
    {
        float cx0 = center0.x, cx1 = center1.x;
        float cy0 = center0.y, cy1 = center1.y;

        // Find the distance between the centers.
        float dx = cx0 - cx1;
        float dy = cy0 - cy1;
        double dist = Mathf.Sqrt(dx * dx + dy * dy);

        // See how many solutions there are.
        if (dist > radius0 + radius1)
        {
            // No solutions, the circles are too far apart.
            intersection1 = Vector3.zero;
            intersection2 = Vector3.zero;
            return 0;
        }
        else if (dist < Mathf.Abs(radius0 - radius1))
        {
            // No solutions, one circle contains the other.
            intersection1 = Vector3.zero;
            intersection2 = Vector3.zero;
            return 0;
        }
        else if ((dist == 0) && (radius0 == radius1))
        {
            // No solutions, the circles coincide.
            intersection1 = Vector3.zero;
            intersection2 = Vector3.zero;
            return 0;
        }
        else
        {
            // Find a and h.
            double a = (radius0 * radius0 -
                radius1 * radius1 + dist * dist) / (2 * dist);
            double h = Mathf.Sqrt((float)(radius0 * radius0 - a * a));

            // Find P2.
            double cx2 = cx0 + a * (cx1 - cx0) / dist;
            double cy2 = cy0 + a * (cy1 - cy0) / dist;

            // Get the points P3.
            intersection1 = new Vector3(
                (float)(cx2 + h * (cy1 - cy0) / dist),
                (float)(cy2 - h * (cx1 - cx0) / dist));
            intersection2 = new Vector3(
                (float)(cx2 - h * (cy1 - cy0) / dist),
                (float)(cy2 + h * (cx1 - cx0) / dist));

            // See if we have 1 or 2 solutions.
            if (dist == radius0 + radius1) return 1;
            return 2;
        }
    }

    // intersectionOf3Circles(IC0C1, IC0C2, IC1C2) returns the midpoint of the close intersections of three circles
    private static Vector3 intersectionOf3Circles(Vector3[] IC0C1, Vector3[] IC0C2, Vector3[] IC1C2)
    {
        // Select between intersection point 1 and intersection point2 for each two circles using bit manpulation technique.
        int best_premiutation = 0;
        float min = float.MaxValue;
        for (int i = 0; i < 8; i++)
        {
            float d1 = Vector3.Distance(IC0C1[i & 1], IC0C2[(i >> 1) & 1]);
            float d2 = Vector3.Distance(IC0C1[i & 1], IC1C2[(i >> 2) & 1]);
            float d3 = Vector3.Distance(IC0C2[(i >> 1) & 1], IC1C2[(i >> 2) & 1]);
            float sum = d1 + d2 + d3;
            if (sum < min)
            {
                min = sum;
                best_premiutation = i;
            }
        }

        //Get the midpoint of the 3 intersections
        float userX = (IC0C1[best_premiutation & 1].x + IC0C2[(best_premiutation >> 1) & 1].x + IC1C2[(best_premiutation >> 2) & 1].x) / 3;
        float userY = (IC0C1[best_premiutation & 1].y + IC0C2[(best_premiutation >> 1) & 1].y + IC1C2[(best_premiutation >> 2) & 1].y) / 3;
        return new Vector3(userX, userY);
    }


    // getUserLocation(center0, radius0, center1, radius1, center2, radius2, out userLocation) returns true if the userLocation could be determined
    public static bool getUserLocation(Vector3 center0, float radius0, Vector3 center1, float radius1, Vector3 center2, float radius2, out Vector3 userLocation)
    {
        // Each array represent the 2 intersecting points between 2 circles.

        // Intersecting points between circle0  and circle1.
        Vector3[] IC0C1 = new Vector3[2];

        // Intersecting points between circle0  and circle2.
        Vector3[] IC0C2 = new Vector3[2];

        // Intersecting points between circle1  and circle2.
        Vector3[] IC1C2 = new Vector3[2];

        userLocation = Vector3.zero;

        // If number of intersting points == 0 this means there is no intersecting points between 2 circles as there is an error happened while calculating the distance.
        if (intersectionOf2Circles(center0, radius0, center1, radius1, out IC0C1[0], out IC0C1[1]) == 0) return false;
        if (intersectionOf2Circles(center0, radius0, center2, radius2, out IC0C2[0], out IC0C2[1]) == 0) return false;
        if (intersectionOf2Circles(center1, radius1, center2, radius2, out IC1C2[0], out IC1C2[1]) == 0) return false;

        userLocation = intersectionOf3Circles(IC0C1, IC0C2, IC1C2);
        return true;
    }
}