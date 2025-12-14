import { useQuery } from "@tanstack/react-query";

import positions from "../data/positions";
import type { Response } from "../services/api-client";
import type { Position } from "../services/positionService";
import positionService from "../services/positionService";

const usePositions = () =>
  useQuery<Response<Position>, Error>({
    queryKey: ["positions"],
    queryFn: () => positionService.getAll(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    initialData: positions,
  });
export default usePositions;
