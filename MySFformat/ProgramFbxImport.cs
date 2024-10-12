using Assimp;
using SoulsFormats;
using SoulsFormats.KF4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace MySFformat
{
    static partial class Program
    {
        //Current, best version can load FBX, OBJ, DAE etc.
        //Use assimp library
        static void ImportFBX()
        {
            AssimpContext importer = new AssimpContext();

            var openFileDialog2 = new OpenFileDialog();
            if (openFileDialog2.ShowDialog() != DialogResult.OK)
                return;

            string res = openFileDialog2.FileName;

            //Prepare bone name convertion table:
            string assemblyPath = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location
            );
            string[] convertStrlines = File.ReadAllText(assemblyPath + "\\boneConvertion.ini").Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );
            Dictionary<string, string> convertionTable = new Dictionary<string, string>();
            for (int i2 = 0; i2 + 1 < convertStrlines.Length; i2++)
            {
                string target = convertStrlines[i2];
                if (target == null)
                    continue;
                if (target.IndexOf('#') == 0)
                    continue;
                Console.WriteLine(target + "->" + convertStrlines[i2 + 1]);
                convertionTable.Add(target, convertStrlines[i2 + 1]);
                i2++;
            }

            //Table prepartion finished

            Scene model = importer.ImportFile(res, PostProcessSteps.CalculateTangentSpace); // PostProcessPreset.TargetRealTimeMaximumQuality

            MessageBox.Show($"Meshes count:{model.Meshes.Count}, Material count:{model.MaterialCount}");

            boneParentList = new Dictionary<string, string>();
            //Build the parent list of bones.

            printNodeStruct(model.RootNode);

            //First, added a custom default layout.
            int layoutCount = targetFlver.BufferLayouts.Count;

            targetFlver.BufferLayouts.Add(new FLVER.BufferLayout
            {
                new FLVER.BufferLayout.Member(
                    0,
                    0,
                    FLVER.BufferLayout.MemberType.Float3,
                    FLVER.BufferLayout.MemberSemantic.Position,
                    0
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    12,
                    FLVER.BufferLayout.MemberType.Byte4B,
                    FLVER.BufferLayout.MemberSemantic.Normal,
                    0
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    16,
                    FLVER.BufferLayout.MemberType.Byte4B,
                    FLVER.BufferLayout.MemberSemantic.Tangent,
                    0
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    20,
                    FLVER.BufferLayout.MemberType.Byte4B,
                    FLVER.BufferLayout.MemberSemantic.Tangent,
                    1
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    24,
                    FLVER.BufferLayout.MemberType.Byte4B,
                    FLVER.BufferLayout.MemberSemantic.BoneIndices,
                    0
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    28,
                    FLVER.BufferLayout.MemberType.Byte4C,
                    FLVER.BufferLayout.MemberSemantic.BoneWeights,
                    0
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    32,
                    FLVER.BufferLayout.MemberType.Byte4C,
                    FLVER.BufferLayout.MemberSemantic.VertexColor,
                    1
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    36,
                    FLVER.BufferLayout.MemberType.UVPair,
                    FLVER.BufferLayout.MemberSemantic.UV,
                    0
                )
            });

            int materialCount = targetFlver.Materials.Count;

            bool setAmbientAsDiffuse = false;

            bool flipYZ = MessageBox.Show(
                "Switch YZ axis values? \n It may help importing some fbx files.",
                "Set", MessageBoxButtons.YesNo ) == DialogResult.Yes;

            bool setLOD = true;

            List<FLVER.Texture> defaultTextures = new List<FLVER.Texture>
            {
                new FLVER.Texture("g_DiffuseTexture",
                    "texture.tif",
                    1,1,1,true,0,0,0),
                new FLVER.Texture("SAT_Equip_snp_Texture2D_1_BlendMask",
                    "N:\\FDP\\data\\Other\\SysTex\\SYSTEX_DummyBurn_m.tif",
                    1,1,0,false,0,0,0),
                new FLVER.Texture("SAT_Equip_snp_Texture2D_0_EmissiveMap_0",
                    "N:\\FDP\\data\\Other\\SysTex\\SYSTEX_DummyBurn_em.tif",
                    1,1,0,false,0,0,0),
                new FLVER.Texture("SAT_Equip_snp_Texture2D_2_DamageNormal",
                    "N:\\FDP\\data\\Other\\SysTex\\SYSTEX_DummyDamagedNormal.tif",
                    1,1,0,false,0,0,0),
                new FLVER.Texture("g_BumpmapTexture",
                    "b.tif",
                    1,1,1,true,0,0,0),
                new FLVER.Texture("g_BumpmapTexture",
                    "s.tif",
                    1,1,1,true,0,0,0)
            };

            foreach (var material in model.Materials)
            {
                
                FLVER.Material newMaterial = new FLVER.Material
                {
                    Name = res.Substring(res.LastIndexOf('\\') + 1) + "_" + material.Name,
                    MTD = "P[ARSN]_e.mtd",
                    Flags = 1342,
                    Textures = defaultTextures,
                    GXBytes = new byte[] { 71, 88, 77, 68, 242, 0, 0, 0, 28, 0, 0, 0, 1, 0, 0, 0, 31, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 71, 88, 48, 48, 100, 0, 0, 0, 44, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 88, 48, 52, 100, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 160, 64, 0, 0, 128, 63, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 160, 64, 0, 0, 0, 63, 0, 0, 128, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 88, 56, 48, 100, 0, 0, 0, 28, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 71, 88, 56, 49, 100, 0, 0, 0, 56, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 127, 100, 0, 0, 0, 12, 0, 0, 0 }
                };
                
                if (setAmbientAsDiffuse)
                {
                    if (material.HasTextureEmissive)
                        SetFlverMatPath(newMaterial, "g_DiffuseTexture",
                            FindFileName(material.TextureEmissive.FilePath) + ".tif");
                }
                else if (material.HasTextureDiffuse) //g_DiffuseTexture
                    SetFlverMatPath(newMaterial, "g_DiffuseTexture",
                        FindFileName(material.TextureDiffuse.FilePath) + ".tif");

                if (material.HasTextureNormal) //g_BumpmapTexture
                    SetFlverMatPath(newMaterial, "g_BumpmapTexture",
                        FindFileName(material.TextureNormal.FilePath) + ".tif");

                if (material.HasTextureSpecular) //g_SpecularTexture
                    SetFlverMatPath(newMaterial, "g_SpecularTexture",
                        FindFileName(material.TextureSpecular.FilePath) + ".tif");
                targetFlver.Materials.Add(newMaterial);
            }

            foreach (var mesh in model.Meshes)
            {
                FLVER.Mesh newMesh = new FLVER.Mesh
                {
                    MaterialIndex = 0,
                    BoneIndices = new List<int> { 0, 1 },
                    BoundingBoxMax = new Vector3(1, 1, 1),
                    BoundingBoxMin = new Vector3(-1, -1, -1),
                    BoundingBoxUnk = new Vector3(),
                    Unk1 = 0,
                    DefaultBoneIndex = 0,
                    Dynamic = true,
                    VertexBuffers = new List<FLVER.VertexBuffer> { new FLVER.VertexBuffer(0, layoutCount, -1) },
                    Vertices = new List<FLVER.Vertex>()
                };

                List<List<int>> verticesBoneIndices = new List<List<int>>();
                List<List<float>> verticesBoneWeights = new List<List<float>>();

                //If it has bones, then record the bone weight info
                if (mesh.HasBones)
                {
                    for (int i = 0; i < mesh.VertexCount; i++)
                    {
                        verticesBoneIndices.Add(new List<int>());
                        verticesBoneWeights.Add(new List<float>());
                    }

                    for (int i = 0; i < mesh.BoneCount; i++)
                    {
                        string boneName = mesh.Bones[i].Name;
                        int boneIndex = FindFLVER_Bone(targetFlver, boneName);

                        if (convertionTable.ContainsKey(mesh.Bones[i].Name))
                        {
                            boneName = convertionTable[boneName];
                            boneIndex = FindFLVER_Bone(targetFlver, boneName);
                        }
                        else
                        {
                            Console.WriteLine("Cannot find ->" + boneName);
                            //If cannot find a corresponding boneName in convertion.ini then
                            //if such bone can not be found in flver, then check its parent to see if it can be convert to its parent bone.
                            //check up to 5th grand parent.
                            for (int bp = 0; boneIndex == -1 && bp < boneFindParentTimes; bp++)
                                if (boneParentList.ContainsValue(boneName) && boneParentList[boneName] != null)
                                {
                                    boneName = boneParentList[boneName];
                                    if (convertionTable.ContainsKey(boneName))
                                        boneName = convertionTable[boneName];
                                    boneIndex = FindFLVER_Bone(targetFlver, boneName);
                                }
                        }
                        if (boneIndex == -1)
                            boneIndex = 0;
                        for (int j = 0; j < mesh.Bones[i].VertexWeightCount; j++)
                        {
                            var vertexWeight = mesh.Bones[i].VertexWeights[j];

                            verticesBoneIndices[vertexWeight.VertexID].Add(boneIndex);
                            verticesBoneWeights[vertexWeight.VertexID].Add(vertexWeight.Weight);
                        }
                    }
                }

                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    var vit = mesh.Vertices[i];
                    var channels = mesh.TextureCoordinateChannels[0];

                    var uv1 = new Vector3D();
                    var uv2 = new Vector3D();

                    if (channels != null && mesh.TextureCoordinateChannelCount > 0)
                    {
                        uv1 = getMyV3D(channels[i]);
                        uv1.Y = 1 - uv1.Y;
                        uv2 = getMyV3D(channels[i]);
                        uv2.Y = 1 - uv2.Y;
                    }

                    var normal = new Vector3D(0, 1, 0);
                    if (mesh.HasNormals && mesh.Normals.Count > i)
                        normal = getMyV3D(mesh.Normals[i]).normalize();

                    var tangent = new Vector3D(1, 0, 0);
                    if (mesh.Tangents.Count > i)
                        tangent = getMyV3D(mesh.Tangents[i]).normalize();
                    else if (mesh.HasNormals && mesh.Normals.Count > i)
                        tangent = new Vector3D(
                            crossPorduct(
                                getMyV3D(mesh.Normals[i]).normalize().toXnaV3(),
                                normal.toXnaV3()
                            )
                        ).normalize();

                    FLVER.Vertex v = generateVertex(
                        new Vector3(vit.X, vit.Y, vit.Z),
                        uv1.toNumV3(),
                        uv2.toNumV3(),
                        normal.toNumV3(),
                        tangent.toNumV3(),
                        1
                    );

                    if (flipYZ)
                        v = generateVertex(
                            new Vector3(vit.X, vit.Z, vit.Y),
                            uv1.toNumV3(),
                            uv2.toNumV3(),
                            new Vector3(normal.X, normal.Z, normal.Y),
                            new Vector3(tangent.X, tangent.Z, tangent.Y),
                            1
                        );

                    if (mesh.HasBones)
                        for (int j = 0; j < verticesBoneIndices[i].Count && j < 4; j++)
                        {
                            v.BoneIndices[j] = (verticesBoneIndices[i])[j];
                            v.BoneWeights[j] = (verticesBoneWeights[i])[j];
                        }
                    newMesh.Vertices.Add(v);
                }

                List<uint> faceIndexs = new List<uint>();
                for (int i = 0; i < mesh.FaceCount; i++)
                {
                    if (flipYZ)
                    {
                        if (mesh.Faces[i].Indices.Count == 3)
                        {
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[0]);
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[1]);
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[2]);
                        }
                        else if (mesh.Faces[i].Indices.Count == 4)
                        {
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[0]);
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[1]);
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[2]);

                            faceIndexs.Add((uint)mesh.Faces[i].Indices[2]);
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[3]);
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[0]);
                        }
                    }
                    else
                    {
                        if (mesh.Faces[i].Indices.Count == 3)
                        {
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[0]);
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[2]);
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[1]);
                        }
                        else if (mesh.Faces[i].Indices.Count == 4)
                        {
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[0]);
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[2]);
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[1]);

                            faceIndexs.Add((uint)mesh.Faces[i].Indices[2]);
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[0]);
                            faceIndexs.Add((uint)mesh.Faces[i].Indices[3]);
                        }
                    }
                }
                newMesh.FaceSets = new List<FLVER.FaceSet>{ generateBasicFaceSet() };
                newMesh.FaceSets[0].Vertices = faceIndexs.ToArray();
                if (newMesh.FaceSets[0].Vertices.Length > 65534)
                {
                    Console.WriteLine("There are more than 65535 vertices in a newMesh , switch to 32 bits index size mode.");
                    newMesh.FaceSets[0].IndexSize = 32;
                }

                if (setLOD == true)
                {//Special thanks to Meowmaritus
                    newMesh.FaceSets.Add(generateBasicFaceSet(
                        FLVER.FaceSet.FSFlags.LodLevel1,
                        newMesh.FaceSets[0].IndexSize,
                        (uint[])newMesh.FaceSets[0].Vertices.Clone()));

                    newMesh.FaceSets.Add(generateBasicFaceSet(
                        FLVER.FaceSet.FSFlags.LodLevel2,
                        newMesh.FaceSets[0].IndexSize,
                        (uint[])newMesh.FaceSets[0].Vertices.Clone()));

                    newMesh.FaceSets.Add(generateBasicFaceSet(
                        FLVER.FaceSet.FSFlags.Unk80000000,
                        newMesh.FaceSets[0].IndexSize,
                        (uint[])newMesh.FaceSets[0].Vertices.Clone()));

                    newMesh.FaceSets.Add(generateBasicFaceSet(
                        FLVER.FaceSet.FSFlags.LodLevel1 | FLVER.FaceSet.FSFlags.Unk80000000,
                        newMesh.FaceSets[0].IndexSize,
                        (uint[])newMesh.FaceSets[0].Vertices.Clone()));

                    newMesh.FaceSets.Add(generateBasicFaceSet(
                        FLVER.FaceSet.FSFlags.LodLevel2 | FLVER.FaceSet.FSFlags.Unk80000000,
                        newMesh.FaceSets[0].IndexSize,
                        (uint[])newMesh.FaceSets[0].Vertices.Clone()));

                    //unk8000000000 is the motion blur
                }

                newMesh.MaterialIndex = materialCount + mesh.MaterialIndex;

                targetFlver.Meshes.Add(newMesh);
            }

            MessageBox.Show("Model imported successfully! PLease click modify to save it!");
            UpdateVertices();
        }

        public static void SetFlverMatPath(FLVER.Material mesh, string typeName, string newPath)
        {
            for (int i = 0; i < mesh.Textures.Count; i++)
                if (mesh.Textures[i].Type == typeName)
                {
                    mesh.Textures[i].Path = newPath;
                    return;
                }

            mesh.Textures.Add(new FLVER.Texture
            {
                Type = typeName,
                Path = newPath,
                ScaleX = 1,
                ScaleY = 1,
                Unk10 = 1,
                Unk11 = true
            });
        }

    }
}
