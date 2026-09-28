using System.Collections.Generic;
using CEOWars.Core;
using UnityEngine;

namespace CEOWars.Game
{
    public class OfficeWorld : MonoBehaviour
    {
        private readonly List<GameObject> generated = new List<GameObject>();
        private Material floorMat;
        private Material wallMat;
        private Material deskMat;
        private Material chairMat;
        private Material employeeMat;
        private Material screenMat;

        public void Build(GameState state)
        {
            ClearGenerated();
            EnsureMaterials();

            var size = Mathf.Clamp(8f + state.officeLevel * 1.2f, 9f, 16f);
            Cube("Floor", new Vector3(0, -0.25f, 0), new Vector3(size, 0.5f, size), floorMat);
            Cube("BackWall", new Vector3(0, 2f, size / 2f), new Vector3(size, 4.5f, 0.3f), wallMat);
            Cube("SideWall", new Vector3(-size / 2f, 2f, 0), new Vector3(0.3f, 4.5f, size), wallMat);

            var desksToShow = Mathf.Clamp(state.employees, 1, 12);
            var cols = 3;
            for (var i = 0; i < desksToShow; i++)
            {
                var row = i / cols;
                var col = i % cols;
                var x = -2.8f + col * 2.8f;
                var z = -2.5f + row * 2.2f;
                BuildDesk(new Vector3(x, 0, z), i);
            }

            BuildExecutiveDesk(new Vector3(1.8f, 0, 3.0f));

            if (state.officeLevel >= 2) BuildPlant(new Vector3(-3.5f, 0, 3.2f));
            if (state.officeLevel >= 3) BuildLounge(new Vector3(3.8f, 0, -3.2f));
            if (state.officeLevel >= 4) BuildMeetingTable(new Vector3(3.2f, 0, 2.0f));
        }

        private void BuildDesk(Vector3 root, int index)
        {
            Cube($"Desk_{index}", root + new Vector3(0, 0.65f, 0), new Vector3(1.8f, 0.16f, 0.8f), deskMat);
            Cube($"LegA_{index}", root + new Vector3(-0.7f, 0.3f, 0), new Vector3(0.12f, 0.6f, 0.6f), deskMat);
            Cube($"LegB_{index}", root + new Vector3(0.7f, 0.3f, 0), new Vector3(0.12f, 0.6f, 0.6f), deskMat);
            Cube($"Monitor_{index}", root + new Vector3(0, 1.15f, 0.05f), new Vector3(0.72f, 0.5f, 0.08f), screenMat);
            Cube($"Chair_{index}", root + new Vector3(0, 0.5f, -0.75f), new Vector3(0.7f, 0.85f, 0.55f), chairMat);
            Capsule($"Employee_{index}", root + new Vector3(0, 1.15f, -0.7f), new Vector3(0.55f, 0.9f, 0.55f), employeeMat);
        }

        private void BuildExecutiveDesk(Vector3 root)
        {
            Cube("ExecutiveDesk", root + new Vector3(0, 0.7f, 0), new Vector3(2.6f, 0.22f, 1.0f), deskMat);
            Cube("ExecutiveMonitor", root + new Vector3(0, 1.25f, 0.1f), new Vector3(0.9f, 0.62f, 0.09f), screenMat);
            Cube("ExecutiveChair", root + new Vector3(0, 0.65f, -1f), new Vector3(0.9f, 1.1f, 0.7f), chairMat);
        }

        private void BuildPlant(Vector3 root)
        {
            Cube("Planter", root + new Vector3(0, 0.35f, 0), new Vector3(0.7f, 0.7f, 0.7f), chairMat);
            Capsule("Plant", root + new Vector3(0, 1.1f, 0), new Vector3(0.65f, 1.2f, 0.65f), employeeMat);
        }

        private void BuildLounge(Vector3 root)
        {
            Cube("Sofa", root + new Vector3(0, 0.45f, 0), new Vector3(2.8f, 0.9f, 0.9f), chairMat);
            Cube("CoffeeTable", root + new Vector3(0, 0.3f, 1.2f), new Vector3(1.6f, 0.18f, 0.8f), deskMat);
        }

        private void BuildMeetingTable(Vector3 root)
        {
            Cube("MeetingTable", root + new Vector3(0, 0.65f, 0), new Vector3(2.6f, 0.18f, 1.5f), deskMat);
        }

        private void EnsureMaterials()
        {
            if (floorMat != null) return;
            floorMat = MakeMaterial(new Color(0.12f, 0.15f, 0.20f));
            wallMat = MakeMaterial(new Color(0.19f, 0.23f, 0.30f));
            deskMat = MakeMaterial(new Color(0.42f, 0.28f, 0.18f));
            chairMat = MakeMaterial(new Color(0.08f, 0.09f, 0.12f));
            employeeMat = MakeMaterial(new Color(0.75f, 0.16f, 0.12f));
            screenMat = MakeMaterial(new Color(0.10f, 0.55f, 0.75f), true);
        }

        private Material MakeMaterial(Color color, bool emission = false)
        {
            var shader = Shader.Find("Standard");
            var mat = new Material(shader) { color = color };
            if (emission)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 0.6f);
            }
            return mat;
        }

        private GameObject Cube(string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            generated.Add(go);
            return go;
        }

        private GameObject Capsule(string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.SetParent(transform);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            generated.Add(go);
            return go;
        }

        private void ClearGenerated()
        {
            foreach (var go in generated)
            {
                if (go != null) Destroy(go);
            }
            generated.Clear();
        }
    }
}
