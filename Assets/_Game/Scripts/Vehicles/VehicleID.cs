namespace Vehicles
{
    /// <summary>
    /// Identifies a vehicle. Used instead of a raw string id everywhere in the game.
    ///
    /// IMPORTANT: values are persisted as ints, so always assign explicit numbers and only
    /// APPEND new vehicles. Never reorder or reuse a number, or existing saves will point at
    /// the wrong vehicle.
    /// </summary>
    public enum VehicleID
    {
        None = 0,
        Supra = 1, // supra          - Toyota Supra      (Sport JDM)
        GTR_R35 = 2, // gtr            - Nissan GT-R R35   (Sport)
        RX7 = 3, // rx7            - Mazda RX-7        (JDM drift)
        SilviaS15 = 4, // s15_ext        - Nissan Silvia S15 (JDM drift)
        MustangGT500 = 5, // GT500          - Ford Mustang GT500(Muscle)
        G63 = 6, // G63            - Mercedes G63      (SUV luxury)
        Challenger = 7, // challenger_ext - Dodge Challenger  (Muscle)
        Bronco = 8, // bronco_ext     - Ford Bronco       (SUV offroad)
        M4 = 9, // m4             - BMW M4            (Performance coupe)
        FType = 10, // ftype_ext      - Jaguar F-Type     (European GT)
        Ferrari488 = 11, // F488           - Ferrari 488       (Super)
        FerrariSF90 = 12, // sf90_ext       - Ferrari SF90      (Super hybrid)
        AstonMartinDBS = 13, // dbs_ext        - Aston Martin DBS  (Super GT)
        Huayra = 14, // huayra_ext     - Pagani Huayra     (Hyper)
        Fenyr = 15, // fenyr_ext      - W Motors Fenyr    (Hyper)
        // Add more vehicles here, appending new explicit values.
    }
}