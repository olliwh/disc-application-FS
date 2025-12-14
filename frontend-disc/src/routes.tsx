import { createBrowserRouter } from "react-router-dom";

import EmployeeProfile from "./pages/EmployeeProfile";
import ErrorPage from "./pages/ErrorPage";
import Home from "./pages/Home";
import Layout from "./pages/Layout";

const router = createBrowserRouter([
  {
    path: "/",
    element: <Layout />,
    errorElement: <ErrorPage />,
    children: [
      { path: "/", element: <Home /> },
      { path: "/employees/:id", element: <EmployeeProfile /> },
    ],
  },
]);

export default router;
