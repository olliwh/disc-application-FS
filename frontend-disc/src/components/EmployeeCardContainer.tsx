import type { ReactNode } from "react";

import { Box } from "@chakra-ui/react";

interface Props {
  children: ReactNode;
}

const EmployeeCardContainer = ({ children }: Props) => {
  return (
    <Box overflow="hidden" borderRadius={10} height={330}>
      {children}
    </Box>
  );
};

export default EmployeeCardContainer;
