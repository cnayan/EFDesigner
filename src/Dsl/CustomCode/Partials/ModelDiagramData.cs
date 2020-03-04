﻿using System;

namespace Sawczyn.EFDesigner.EFModel
{
   partial class ModelDiagramData
   {
      private EFModelDiagram diagram;

      public EFModelDiagram GetDiagram() { return diagram; }

      public void SetDiagram(EFModelDiagram d) { diagram = d; }

      public static Action<ModelDiagramData> OpenDiagram { get; set; }
      public static Action<EFModelDiagram> CloseDiagram { get; set; }
   }
}