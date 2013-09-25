using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Wpf_newdemo_ok
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
        private Point mLastPos;
        private Point3D _center;
        private Point3D potion = new Point3D(0, 0, 0);
        private bool lflag = false;
        private bool rflag = false;
        Transform3DGroup t3dg;
        TranslateTransform3D move;
		public MainWindow()
		{
			this.InitializeComponent();
  
			// 在此点下面插入创建对象所需的代码。
            World.Transform = new Transform3DGroup();
		}

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (Mouse.GetPosition(this).X > 1000)
            {
                Yaw(true, 1);
            }
            else if(Mouse.GetPosition(this).X < 500)
            {
                Yaw(false, 1);
            }
        }
        private void MouseRight_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            //choice 2
            MeasureModel(body);
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
            //choice 1
            //rflag = true;
            //Point pos = Mouse.GetPosition(viewport);
            //mLastPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
        }
        public void MeasureModel(ModelVisual3D model)
        {
            
            var rect3D = Rect3D.Empty;
            UnionRect(model, ref rect3D);

            _center = new Point3D((rect3D.X + rect3D.SizeX / 2), (rect3D.Y + rect3D.SizeY / 2),
                                  (rect3D.Z + rect3D.SizeZ / 2));
            double radius = (_center - rect3D.Location).Length;
            Point3D position = _center;
            position.Z += radius * 1.2;
            position.X = position.Z;
            Camera.Position = position;
            Camera.LookDirection = _center - position;
            Camera.NearPlaneDistance = radius / 100;
            Camera.FarPlaneDistance = radius * 100;
        }
        private void UnionRect(ModelVisual3D model, ref Rect3D rect3D)    //矩形
        {
            for (int i = 0; i < model.Children.Count; i++)
            {
                var child = model.Children[i] as ModelVisual3D;
                UnionRect(child, ref rect3D);
            }
            if (model.Content != null)
                rect3D.Union(model.Content.Bounds);
        }

        public void Yaw(bool leftRight, double angleDeltaFactor)        //旋转
        {
            var axis = new AxisAngleRotation3D(Camera.UpDirection, leftRight ? angleDeltaFactor : -angleDeltaFactor);
            var rt3D = new RotateTransform3D(axis) { CenterX = _center.X, CenterY = _center.Y, CenterZ = _center.Z };
            Matrix3D matrix3D = rt3D.Value;
            Point3D point3D = Camera.Position;
            Point3D position = matrix3D.Transform(point3D);
            Camera.Position = position;
            Camera.LookDirection = Camera.LookDirection = _center - position;
        }
        public void Move()        //平移
        {
            move = new TranslateTransform3D(new Vector3D(1, 0, 0));
            t3dg.Children.Add(move);
            Camera.Transform = t3dg;
        }

        private void MouseLeft_ButtonDown(object sender, MouseButtonEventArgs e)    //
        {
            lflag = true;
            //得到视角在显示屏幕的坐标
            Point pos = Mouse.GetPosition(viewport);

            mLastPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
        }

        private void MouseLeft_ButtonUp(object sender, MouseButtonEventArgs e)    //mouseleft up
        {
            lflag = false;
        }

        private void Mouse_Move(object sender, MouseEventArgs e)      //鼠标移动
        {
            if (lflag)
            {
                Point pos = Mouse.GetPosition(viewport);
                Point actualPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
                double dx = actualPos.X - mLastPos.X, dy = actualPos.Y - mLastPos.Y;
                //Camera.LookDirection = new Vector3D(Camera.LookDirection.X + dx / 10, Camera.LookDirection.Y + dx / 10, Camera.LookDirection.Z);
                //Camera.UpDirection = new Vector3D(Camera.LookDirection.X + dx / 10000, Camera.LookDirection.Y + dx / 10000, Camera.LookDirection.Z);
                Camera.Position = new Point3D(Camera.Position.X + dx, Camera.Position.Y, Camera.Position.Z);

                mLastPos = actualPos;
            }
            else if (rflag)
            {
                Point pos = Mouse.GetPosition(viewport);
                Point actualPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
                double dx = actualPos.X - mLastPos.X, dy = actualPos.Y - mLastPos.Y;
                Camera.LookDirection = new Vector3D(Camera.LookDirection.X + dx / 1000, Camera.LookDirection.Y + dy / 1000, Camera.LookDirection.Z);
                double mouseAngle = 0;
                if (dx != 0 && dy != 0)
                {
                    mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
                    if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
                    else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
                    else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
                }
                else if (dx == 0 && dy != 0) mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
                else if (dx != 0 && dy == 0) mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;

                double axisAngle = mouseAngle + Math.PI / 2;

                Vector3D axis = new Vector3D(Math.Cos(axisAngle) * 4, Math.Sin(axisAngle) * 4, 0);

                double rotation = 0.01 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
                Transform3DGroup group = World.Transform as Transform3DGroup;
                QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(axis, rotation * 180 / Math.PI));
                group.Children.Add(new RotateTransform3D(r));  
                mLastPos = actualPos;
            }
        }

        private void Mouse_Wheel(object sender, MouseWheelEventArgs e)     //滚轮旋转
        {
            double dx = 0;
            double dy = 0;
            double dz = 0;
            double length = Math.Sqrt(Math.Pow(Camera.LookDirection.X, 2) + Math.Pow(Camera.LookDirection.Y, 2) + Math.Pow(Camera.LookDirection.Z, 2));
            dx = Camera.LookDirection.X / length * e.Delta / 10D;
            dy = Camera.LookDirection.Y / length * e.Delta / 10D;
            dz = Camera.LookDirection.Z / length * e.Delta / 10D;
            Camera.Position = new Point3D(Camera.Position.X + dx, Camera.Position.Y + dy, Camera.Position.Z + dz);
        }

        private void MouseRight_ButtonUp(object sender, MouseButtonEventArgs e)      //mouseright up
        {
            rflag = false;
        }
	}
}