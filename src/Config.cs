using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudicaModding
{
    public class Config
    {
        public bool activated = true;
        public float positionSmoothing = 0.005f;
        public float rotationSmoothing = 0.005f;
        public float camHeight = 1.0f;
        public float camDistance = 5.0f;
        public float camRotation = 0.0f;
        public float camOffset = 5.0f;
    }
}
