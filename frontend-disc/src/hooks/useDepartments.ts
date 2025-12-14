import { useQuery } from "@tanstack/react-query";

import departments from "../data/departments";
import type { Response } from "../services/api-client";
import departmentService, {
  type Department,
} from "../services/departmentService";

const useDepartments = () =>
  useQuery<Response<Department>, Error>({
    queryKey: ["departments"],
    queryFn: () => departmentService.getAll(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    initialData: departments,
  });
export default useDepartments;
