using System;

namespace MySFformat
{
    //we use -> order, the reverse order of MAYA expression, the front one tis the parent
    public enum RotationOrder
    {
        XYZ,
        XZY,
        YXZ,
        YZX,
        ZXY,
        ZYX,
    }

    class Transform3D
    {
        public string name = "";
        public Vector3D position = new Vector3D();

        //rotation unit: degree
        public Vector3D rotation = new Vector3D();

        public Vector3D scale = new Vector3D(1, 1, 1);

        public Transform3D parent = null;

        public RotationOrder rotOrder = RotationOrder.YZX;

        public Vector3D getGlobalOrigin()
        {
            Vector3D org = new Vector3D();
            Matrix3D transMatrix = new Matrix3D();

            {
                Matrix3D rs = Matrix3D.generateScaleMatrix(scale.X, scale.Y, scale.Z);
                Matrix3D rx = Matrix3D.generateRotXMatrix(rotation.X);
                Matrix3D ry = Matrix3D.generateRotYMatrix(rotation.Y);
                Matrix3D rz = Matrix3D.generateRotZMatrix(rotation.Z);
                Matrix3D pos = Matrix3D.generateTranslationMatrix(
                    position.X,
                    position.Y,
                    position.Z
                );

                if (rotOrder == RotationOrder.XYZ)
                    transMatrix = pos * (rx * (ry * (rz * rs)));
                if (rotOrder == RotationOrder.XZY)
                    transMatrix = pos * (rx * (rz * (ry * rs)));
                if (rotOrder == RotationOrder.YXZ)
                    transMatrix = pos * (ry * (rx * (rz * rs)));
                if (rotOrder == RotationOrder.YZX)
                    transMatrix = pos * (ry * (rz * (rx * rs)));
                if (rotOrder == RotationOrder.ZXY)
                    transMatrix = pos * (rz * (rx * (ry * rs)));
                if (rotOrder == RotationOrder.ZYX)
                    transMatrix = pos * (rz * (ry * (rx * rs)));
            }

            Transform3D parentT = parent;
            while (parentT != null)
            {
                Matrix3D rx = Matrix3D.generateRotXMatrix(parentT.rotation.X);
                Matrix3D ry = Matrix3D.generateRotYMatrix(parentT.rotation.Y);
                Matrix3D rz = Matrix3D.generateRotZMatrix(parentT.rotation.Z);
                Matrix3D pos = Matrix3D.generateTranslationMatrix(
                    parentT.position.X,
                    parentT.position.Y,
                    parentT.position.Z
                );

                transMatrix =
                    Matrix3D.generateScaleMatrix(parentT.scale.X, parentT.scale.Y, parentT.scale.Z)
                    * transMatrix;

                if (rotOrder == RotationOrder.XYZ)
                    transMatrix = pos * (rx * (ry * (rz * transMatrix)));
                if (rotOrder == RotationOrder.XZY)
                    transMatrix = pos * (rx * (rz * (ry * transMatrix)));
                if (rotOrder == RotationOrder.YXZ)
                    transMatrix = pos * (ry * (rx * (rz * transMatrix)));
                if (rotOrder == RotationOrder.YZX)
                    transMatrix = pos * (ry * (rz * (rx * transMatrix)));
                if (rotOrder == RotationOrder.ZXY)
                    transMatrix = pos * (rz * (rx * (ry * transMatrix)));
                if (rotOrder == RotationOrder.ZYX)
                    transMatrix = pos * (rz * (ry * (rx * transMatrix)));

                if (parent.parent == null)
                    break;
                // transMatrix = pos * (rx * (ry * (rz * transMatrix)));
                parentT = parentT.parent;
            }
            return Matrix3D.matrixTimesVector3D(transMatrix, org);
        }

        public void setRotationInRad(Vector3D v3d)
        {
            rotation.X = (float)(v3d.X / Math.PI * 180);
            rotation.Y = (float)(v3d.Y / Math.PI * 180);
            rotation.Z = (float)(v3d.Z / Math.PI * 180);
        }
    }
}
