using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Ej02
{
    public partial class Form1 : Form
    {
        // Nuestro árbol de nombres vive aquí
        ArbolBinario arbol = new ArbolBinario();

        public Form1()
        {
            InitializeComponent();

            // Al iniciar la pantalla llenamos el ComboBox con las opciones de recorrido
            // Lo hacemos aquí para que funcione aunque el evento Load no esté configurado
            CargarOpcionesOrden();
        }

        // Llena el ComboBox con las tres formas clásicas de recorrer un árbol
        private void CargarOpcionesOrden()
        {
            cmbOrden.Items.Clear();
            cmbOrden.Items.AddRange(new object[] { "Pre", "Ino", "Post" });
            cmbOrden.SelectedIndex = 1;                 // Dejamos Ino como opción por defecto
            cmbOrden.DropDownStyle = ComboBoxStyle.DropDownList;   // Evita que escriban texto libre
        }

        // Inserta un nombre en el árbol y actualiza la vista estructural en el TreeView
        private void btnInsertar_Click(object sender, EventArgs e)
        {
            string nombre = txtNombre.Text.Trim();

            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("Ingrese un nombre válido.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            arbol.Insertar(nombre);
            lblResultado.Text = $"Se insertó '{nombre}' correctamente";
            txtNombre.Clear();

            // Mostramos la estructura actual del árbol en el TreeView
            MostrarEstructura();
        }

        // Busca un nombre y si existe lo resalta en el TreeView
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            string nombre = txtNombre.Text.Trim();

            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("Ingrese un nombre para buscar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (arbol.EstaVacio())
            {
                MessageBox.Show("El árbol está vacío.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool encontrado = arbol.Buscar(nombre);
            lblResultado.Text = encontrado ? $"'{nombre}' SÍ está en el árbol" : $"'{nombre}' NO se encontró";

            // Si existe, lo seleccionamos en el TreeView para que el usuario lo vea rápido
            if (encontrado && tvArbol.Nodes.Count > 0)
            {
                var nodoTV = BuscarTreeNode(tvArbol.Nodes[0], nombre);
                if (nodoTV != null)
                {
                    tvArbol.SelectedNode = nodoTV;
                    nodoTV.EnsureVisible();
                    tvArbol.Focus();
                }
            }
        }

        // Muestra el recorrido solicitado en un cuadro de información
        // No cambia el TreeView, solo comunica el resultado con texto
        private void btnMostrar_Click(object sender, EventArgs e)
        {
            if (arbol.EstaVacio())
            {
                MessageBox.Show("El árbol está vacío.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var orden = LeerOrden();          // Tomamos la opción del ComboBox
            var lista = arbol.Obtener(orden); // Obtenemos el recorrido como lista de nombres

            string titulo = orden == Orden.Pre ? "Recorrido Preorden"
                         : orden == Orden.Post ? "Recorrido Postorden"
                         : "Recorrido Inorden";

            // Mostramos el resultado en un cuadro de diálogo informativo
            MessageBox.Show(string.Join(", ", lista), titulo, MessageBoxButtons.OK, MessageBoxIcon.Information);

            lblResultado.Text = $"{titulo} mostrado en pantalla";
        }

        // Lee la opción del ComboBox y la traduce al enum usado por el árbol
        private Orden LeerOrden()
        {
            string texto = cmbOrden.SelectedItem?.ToString()?.Trim().ToLower();
            if (texto == "pre") return Orden.Pre;
            if (texto == "post") return Orden.Post;
            return Orden.Ino;
        }

        // Construye la vista estructural del árbol en el TreeView
        // Esta vista muestra padre e hijos como un árbol real
        private void MostrarEstructura()
        {
            tvArbol.BeginUpdate();
            tvArbol.Nodes.Clear();

            if (!arbol.EstaVacio())
            {
                TreeNode raizTV = CrearNodoTV(arbol.Raiz);
                tvArbol.Nodes.Add(raizTV);
                tvArbol.ExpandAll();  // Abrimos todas las ramas para que se vea completo
            }

            tvArbol.EndUpdate();
        }

        // Convierte un nodo lógico del árbol en un TreeNode visual
        private TreeNode CrearNodoTV(Nodo n)
        {
            if (n == null) return null;

            TreeNode tn = new TreeNode(n.Nombre);
            if (n.Izquierdo != null) tn.Nodes.Add(CrearNodoTV(n.Izquierdo));
            if (n.Derecho != null) tn.Nodes.Add(CrearNodoTV(n.Derecho));
            return tn;
        }

        // Busca un nombre dentro del TreeView de forma recursiva
        private TreeNode BuscarTreeNode(TreeNode raizTV, string nombre)
        {
            if (raizTV == null) return null;

            if (string.Equals(raizTV.Text, nombre, StringComparison.OrdinalIgnoreCase))
                return raizTV;

            foreach (TreeNode hijo in raizTV.Nodes)
            {
                var encontrado = BuscarTreeNode(hijo, nombre);
                if (encontrado != null) return encontrado;
            }

            return null;
        }

        // Nodo simple con nombre e hijos izquierdo y derecho
        public class Nodo
        {
            public string Nombre;
            public Nodo Izquierdo;
            public Nodo Derecho;

            public Nodo(string nombre)
            {
                Nombre = nombre;
            }
        }

        // Tipos de recorrido que soportamos
        public enum Orden { Pre, Ino, Post }

        // Árbol binario de búsqueda con operaciones básicas
        public class ArbolBinario
        {
            private Nodo raiz;

            // Se expone la raíz para poder dibujar la estructura en el TreeView
            public Nodo Raiz => raiz;

            // Informa si no hay datos
            public bool EstaVacio()
            {
                return raiz == null;
            }

            // Inserta manteniendo el criterio alfabético
            public void Insertar(string nombre)
            {
                raiz = InsertarRec(raiz, nombre);
            }

            // Inserción recursiva
            private Nodo InsertarRec(Nodo actual, string nombre)
            {
                if (actual == null) return new Nodo(nombre);

                int comp = string.Compare(nombre, actual.Nombre, StringComparison.OrdinalIgnoreCase);
                if (comp < 0)
                    actual.Izquierdo = InsertarRec(actual.Izquierdo, nombre);
                else if (comp > 0)
                    actual.Derecho = InsertarRec(actual.Derecho, nombre);
                // Si el nombre ya existe no hacemos nada para evitar duplicados

                return actual;
            }

            // Busca un nombre usando la propiedad del árbol binario de búsqueda
            public bool Buscar(string nombre)
            {
                return BuscarRec(raiz, nombre);
            }

            // Búsqueda recursiva
            private bool BuscarRec(Nodo actual, string nombre)
            {
                if (actual == null) return false;

                int comp = string.Compare(nombre, actual.Nombre, StringComparison.OrdinalIgnoreCase);
                if (comp == 0) return true;
                if (comp < 0) return BuscarRec(actual.Izquierdo, nombre);
                return BuscarRec(actual.Derecho, nombre);
            }

            // Devuelve la lista con el recorrido solicitado
            public List<string> Obtener(Orden orden)
            {
                var lista = new List<string>();
                if (orden == Orden.Pre) PreOrden(raiz, lista);
                else if (orden == Orden.Post) PostOrden(raiz, lista);
                else InOrden(raiz, lista);
                return lista;
            }

            // Inorden visita izquierda, visita raíz, visita derecha
            private void InOrden(Nodo actual, List<string> lista)
            {
                if (actual != null)
                {
                    InOrden(actual.Izquierdo, lista);
                    lista.Add(actual.Nombre);
                    InOrden(actual.Derecho, lista);
                }
            }

            // Preorden visita raíz, visita izquierda, visita derecha
            private void PreOrden(Nodo actual, List<string> lista)
            {
                if (actual != null)
                {
                    lista.Add(actual.Nombre);
                    PreOrden(actual.Izquierdo, lista);
                    PreOrden(actual.Derecho, lista);
                }
            }

            // Postorden visita izquierda, visita derecha, visita raíz
            private void PostOrden(Nodo actual, List<string> lista)
            {
                if (actual != null)
                {
                    PostOrden(actual.Izquierdo, lista);
                    PostOrden(actual.Derecho, lista);
                    lista.Add(actual.Nombre);
                }
            }
        }
    }
}