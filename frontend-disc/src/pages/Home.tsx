import { Grid, GridItem, Show } from "@chakra-ui/react";

import DepartmentList from "../components/DepartmentList";
import DiscProfileSelector from "../components/DiscProfileSelector";
import EmployeeGrid from "../components/EmployeeGrid";
import PositionList from "../components/PositionList";
import Weather from "../components/Weather";

function App() {
  return (
    <Grid
      templateAreas={{ base: `"nav" "main"`, lg: `"nav nav" "aside main"` }}
      templateColumns={{ base: "1fr", lg: "200px 1fr" }}
    >
      <Show above="lg">
        <GridItem area="aside">
          <Weather />
          <DepartmentList />
          <PositionList />
        </GridItem>
      </Show>
      <GridItem area="main">
        <DiscProfileSelector />
        <EmployeeGrid />
      </GridItem>
    </Grid>
  );
}

export default App;
