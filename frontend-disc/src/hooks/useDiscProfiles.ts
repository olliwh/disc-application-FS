import { useQuery } from "@tanstack/react-query";

import discprofiles from "../data/discprofiles";
import type { Response } from "../services/api-client";
import type { DiscProfile } from "../services/discProfileService";
import discProfileService from "../services/discProfileService";

const useDiscProfiles = () =>
  useQuery<Response<DiscProfile>, Error>({
    queryKey: ["discProfiles"],
    queryFn: () => discProfileService.getAll(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    initialData: discprofiles,
  });

export default useDiscProfiles;
